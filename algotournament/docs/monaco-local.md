Local Monaco installation

If your network blocks CDN or Monaco fails to load from CDN, place a local copy under `wwwroot/lib/monaco`.

Steps (simple):

1. Download the Monaco Editor `min` build. You can get it from the official CDN or build output (example uses GitHub or npm):

- Using npm (requires Node.js):

```bash
npm install monaco-editor
# then copy node_modules/monaco-editor/min/vs into your project
mkdir -p wwwroot/lib/monaco
cp -R node_modules/monaco-editor/min/vs wwwroot/lib/monaco/
```

- Using a zipped release (manual):

Download the `min` folder from https://github.com/microsoft/monaco-editor or from CDN artifacts and extract to `wwwroot/lib/monaco/vs` so the loader becomes `wwwroot/lib/monaco/vs/loader.js`.

2. Verify the files exist

- `wwwroot/lib/monaco/vs/loader.js`
- `wwwroot/lib/monaco/vs/editor/editor.main.js`

3. Reload the page. The editor script will attempt CDN first and then automatically use `/lib/monaco/vs/loader.js` if present.

Notes

- The local copy may be large; prefer CDN for development unless your environment blocks it.
- If you use a build pipeline, consider copying the `vs` folder as part of your build/publish steps.
