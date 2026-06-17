(function () {
  const monacoCdnBase =
    "https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.52.2/min";
  const monacoLoaderUrl = `${monacoCdnBase}/vs/loader.min.js`;
  const monacoBaseUrl = `${monacoCdnBase}/vs`;

  const localLoaderUrl = "/lib/monaco/vs/loader.js";
  const localBaseUrl = "/lib/monaco/vs";
  const monacoTimeoutMs = 15000;
  const editorInstances = new Map();

  function safeLog(...args) {
    if (window.console && typeof window.console.log === "function") {
      window.console.log("[MonacoLoader]", ...args);
    }
  }

  function safeError(...args) {
    if (window.console && typeof window.console.error === "function") {
      window.console.error("[MonacoLoader]", ...args);
    }
  }

  function loadScript(src, timeoutMs) {
    return new Promise((resolve, reject) => {
      const script = document.createElement("script");
      let timer = null;

      script.src = src;
      script.async = true;
      script.onload = () => {
        clearTimeout(timer);
        resolve();
      };
      script.onerror = () => {
        clearTimeout(timer);
        reject(new Error(`Failed to load ${src}`));
      };

      timer = setTimeout(() => {
        reject(new Error(`Script load timed out: ${src}`));
      }, timeoutMs);

      document.head.appendChild(script);
    });
  }

  function isRequireConfigured() {
    return (
      typeof window.require === "function" &&
      typeof window.require.config === "function"
    );
  }

  function configureRequire(baseUrl) {
    if (!isRequireConfigured()) {
      return;
    }

    try {
      window.require.config({ paths: { vs: baseUrl } });
    } catch (error) {
      safeError("Failed to configure require paths:", error);
    }
  }

  async function loadMonacoWithLoader(loaderUrl, baseUrl) {
    if (!isRequireConfigured()) {
      await loadScript(loaderUrl, monacoTimeoutMs);
    }

    configureRequire(baseUrl);

    return new Promise((resolve, reject) => {
      if (!isRequireConfigured()) {
        reject(new Error("AMD loader is unavailable after script load."));
        return;
      }

      try {
        window.require(
          ["vs/editor/editor.main"],
          () => {
            resolve();
          },
          reject,
        );
      } catch (error) {
        reject(error);
      }
    });
  }

  async function tryLoadMonaco() {
    try {
      safeLog("Loading Monaco from CDN:", monacoLoaderUrl);
      await loadMonacoWithLoader(monacoLoaderUrl, monacoBaseUrl);
      safeLog("Monaco loaded from CDN");
      return;
    } catch (cdnError) {
      safeError("CDN load failed:", cdnError);
    }

    try {
      safeLog("Falling back to local Monaco loader:", localLoaderUrl);
      await loadMonacoWithLoader(localLoaderUrl, localBaseUrl);
      safeLog("Monaco loaded from local fallback");
      return;
    } catch (localError) {
      safeError("Local fallback load failed:", localError);
      throw localError;
    }
  }

  function getMonacoLanguage(modeValue) {
    if (!modeValue) {
      return "plaintext";
    }

    const mode = modeValue.toLowerCase();
    switch (mode) {
      case "cpp":
      case "c++":
      case "cpp17":
      case "cpp20":
        return "cpp";
      case "markdown":
        return "markdown";
      case "plaintext":
      case "text":
        return "plaintext";
      default:
        return mode;
    }
  }

  function debounce(fn, delay) {
    let timeoutId = null;
    return function (...args) {
      clearTimeout(timeoutId);
      timeoutId = window.setTimeout(() => fn.apply(this, args), delay);
    };
  }

  function createNotice(wrapper, message) {
    let notice = wrapper.querySelector(".editor-notice");
    if (!notice) {
      notice = document.createElement("div");
      notice.className = "editor-notice";
      wrapper.appendChild(notice);
    }

    notice.textContent = message;
    return notice;
  }

  function createToolbar(wrapper, title) {
    let toolbar = wrapper.querySelector(".monaco-toolbar");
    if (toolbar) {
      return toolbar;
    }

    toolbar = document.createElement("div");
    toolbar.className = "monaco-toolbar";
    toolbar.innerHTML = `
      <div class="toolbar-left"><span>${title || "Code editor"}</span></div>
      <div class="toolbar-actions"></div>
    `;
    wrapper.insertBefore(toolbar, wrapper.firstChild);
    return toolbar;
  }

  function updateStatus(editor, statusItem) {
    if (!editor || !statusItem) {
      return;
    }

    const position = editor.getPosition() || { lineNumber: 1, column: 1 };
    const model = editor.getModel();
    const chars = model ? model.getValueLength() : 0;

    statusItem.textContent = `Ln ${position.lineNumber}, Col ${position.column} · ${chars.toLocaleString()} chars`;
  }

  function toggleFullscreen(wrapper, editor, button) {
    if (!wrapper || !editor) {
      return;
    }

    const isActive = wrapper.classList.toggle("monaco-fullscreen");
    if (isActive) {
      document.body.style.overflow = "hidden";
      button.textContent = "Exit full screen";
      button.classList.add("active");
    } else {
      document.body.style.overflow = "";
      button.textContent = "Fullscreen";
      button.classList.remove("active");
    }

    window.setTimeout(() => {
      editor.layout();
      editor.focus();
    }, 50);
  }

  function restoreTextareaForFallback(textarea) {
    if (!textarea) {
      return;
    }

    textarea.style.display = "block";
    textarea.style.width = "100%";
    textarea.style.minHeight = "220px";
    textarea.style.marginTop = "1rem";
  }

  function updateTextarea(editor, textarea) {
    if (!editor || !textarea) {
      return;
    }

    textarea.value = editor.getValue();
  }

  function flushAll() {
    editorInstances.forEach((editor, textarea) => {
      updateTextarea(editor, textarea);
    });
  }

  function getStoredValue(storageKey) {
    if (!storageKey) {
      return null;
    }

    try {
      return window.localStorage.getItem(storageKey);
    } catch (error) {
      safeError("Unable to read localStorage:", error);
      return null;
    }
  }

  function saveStoredValue(storageKey, value) {
    if (!storageKey) {
      return;
    }

    try {
      window.localStorage.setItem(storageKey, value);
    } catch (error) {
      safeError("Unable to write localStorage:", error);
    }
  }

  function insertUploadButton(wrapper, editor, inputId) {
    if (!inputId || !editor) {
      return;
    }

    const input = document.getElementById(inputId);
    if (!input || input.type !== "file") {
      return;
    }

    const button = document.createElement("button");
    button.type = "button";
    button.className = "editor-btn";
    button.textContent = "Upload";
    button.addEventListener("click", () => input.click());

    input.addEventListener("change", () => {
      const file = input.files && input.files[0];
      if (!file) {
        return;
      }

      const reader = new FileReader();
      reader.onload = () => {
        if (typeof reader.result === "string") {
          editor.setValue(reader.result);
          editor.focus();
        }
        input.value = "";
      };
      reader.readAsText(file);
    });

    const toolbar = createToolbar(wrapper);
    const actions = toolbar.querySelector(".toolbar-actions");
    if (actions) {
      actions.appendChild(button);
    }
  }

  function createEditor(wrapper, textarea, options) {
    const toolbar = createToolbar(wrapper, options.label || "Code editor");
    const fullscreenButton = document.createElement("button");
    fullscreenButton.type = "button";
    fullscreenButton.className = "editor-btn fullscreen-toggle";
    fullscreenButton.textContent = "Fullscreen";
    fullscreenButton.addEventListener("click", () =>
      toggleFullscreen(wrapper, editor, fullscreenButton),
    );
    const toolbarActions = toolbar.querySelector(".toolbar-actions");
    if (toolbarActions) {
      toolbarActions.appendChild(fullscreenButton);
    }

    const editorContainer = document.createElement("div");
    editorContainer.className = "monaco-editor-container";
    wrapper.appendChild(editorContainer);

    const statusBar = document.createElement("div");
    statusBar.className = "monaco-status-bar";
    statusBar.innerHTML = '<span class="status-item">Editor ready</span>';
    wrapper.appendChild(statusBar);

    const initialValue = textarea.value || "";
    const storageKey = options.autosaveKey || null;
    const savedValue = getStoredValue(storageKey);
    const sourceValue =
      savedValue && initialValue.trim().length === 0
        ? savedValue
        : initialValue;

    const model = monaco.editor.createModel(sourceValue, options.language);
    const editor = monaco.editor.create(editorContainer, {
      model,
      language: options.language,
      theme: "vs-dark",
      automaticLayout: true,
      minimap: { enabled: false },
      scrollBeyondLastLine: false,
      fontSize: 14,
      wordWrap: "on",
    });

    textarea.style.display = "none";
    textarea.dataset.monacoAttached = "true";
    updateTextarea(editor, textarea);

    const statusItem = statusBar.querySelector(".status-item");

    editor.onDidChangeModelContent(
      debounce(() => {
        updateTextarea(editor, textarea);
        if (storageKey) {
          saveStoredValue(storageKey, editor.getValue());
        }
        updateStatus(editor, statusItem);
      }, 250),
    );

    editor.onDidChangeCursorPosition(
      debounce(() => {
        updateStatus(editor, statusItem);
      }, 100),
    );

    if (options.uploadInputId) {
      insertUploadButton(wrapper, editor, options.uploadInputId);
    }

    editorInstances.set(textarea, editor);
    return editor;
  }

  function initializeWrapper(wrapper) {
    const textarea = wrapper.querySelector("textarea");
    if (!textarea) {
      return null;
    }

    const mode = getMonacoLanguage(wrapper.dataset.monacoMode);
    const options = {
      language: mode,
      autosaveKey: wrapper.dataset.monacoAutosaveKey,
      uploadInputId: wrapper.dataset.monacoUploadInput,
      label: wrapper.dataset.monacoLabel,
    };

    return { wrapper, textarea, options };
  }

  function initializeEditors() {
    const wrappers = Array.from(
      document.querySelectorAll(
        ".monaco-editor-wrapper, .monaco-admin-editor-wrapper",
      ),
    );

    return wrappers.map(initializeWrapper).filter(Boolean);
  }

  async function init() {
    const wrappers = initializeEditors();
    if (wrappers.length === 0) {
      return;
    }

    try {
      await tryLoadMonaco();
      wrappers.forEach((entry) => {
        createEditor(entry.wrapper, entry.textarea, entry.options);
      });
    } catch (error) {
      wrappers.forEach((entry) => {
        createNotice(
          entry.wrapper,
          "Monaco is unavailable. The page will continue using the textarea.",
        );
        restoreTextareaForFallback(entry.textarea);
      });
    }

    if (!window.VscodeEditor) {
      window.VscodeEditor = {};
    }
    window.VscodeEditor.flushAll = flushAll;

    wrappers.forEach((entry) => {
      const form = entry.textarea.closest("form");
      if (form && !form.dataset.monacoSubmitHandler) {
        form.addEventListener("submit", () => {
          flushAll();
        });
        form.dataset.monacoSubmitHandler = "true";
      }
    });
  }

  document.addEventListener("DOMContentLoaded", init);
})();
