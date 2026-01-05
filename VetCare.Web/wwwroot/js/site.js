// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.



document.addEventListener('DOMContentLoaded', () => {
    const container = document.getElementById('container');
    const registerBtn = document.getElementById('registerBtn');
    const loginBtn = document.getElementById('loginBtn');

    if (registerBtn && loginBtn && container) {
        registerBtn.addEventListener('click', () => {
            container.classList.add("active");
        });
        loginBtn.addEventListener('click', () => {
            container.classList.remove("active");
        });
    }
});



document.addEventListener('DOMContentLoaded', function () {
    const btnToggle = document.getElementById('toggleSidebar');
    const wrapper = document.getElementById('dashboardWrapper');

    if (btnToggle) {
        btnToggle.addEventListener('click', function () {
            wrapper.classList.toggle('sidebar-collapsed');
        });
    }
});