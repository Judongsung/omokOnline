const isProduction = window.location.hostname.includes("pages.dev");

const CONFIG = {
    SERVER_URL: isProduction
        ? "https://eerie-ricki-judongsung-275ef74a.koyeb.app/omokHub"
        : "http://localhost:5149/omokHub"
}