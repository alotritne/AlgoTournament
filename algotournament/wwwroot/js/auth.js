export const currentUser = {
    username: "tourist",
    rating: 3840
};

// Toggle to false to test Guest Mode
export const isAuthenticated = false;

export function initAuth() {
    const navLinks = document.querySelector('.nav-links');
    if (!navLinks) return;

    const loginBtn = navLinks.querySelector('.login-btn');
    
    if (isAuthenticated && loginBtn) {
        loginBtn.textContent = currentUser.username;
        loginBtn.href = "dashboard-user.html";
        loginBtn.classList.remove('login-btn');
        loginBtn.classList.add('btn', 'btn-secondary');
        loginBtn.style.padding = '0.3rem 1rem';
        
        // Add logout link
        const logout = document.createElement('a');
        logout.href = "#";
        logout.textContent = "logout";
        logout.setAttribute('data-i18n', 'nav.logout');
        logout.onclick = (e) => { 
            e.preventDefault(); 
            alert("Logged out (Mock)"); // This should use i18n in production 
            location.reload(); 
        };
        navLinks.appendChild(logout);
    }
}
