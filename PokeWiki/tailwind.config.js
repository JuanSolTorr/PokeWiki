/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./Views/**/*.cshtml",
        "./Areas/**/*.cshtml",
        "./wwwroot/**/*.js",
        "./Helpers/**/*.cs"
    ],
    theme: {
        extend: {
            colors: {
                'poke-red': '#E3350D',
                'poke-dark': '#212121',
                'poke-light': '#F2F2F2',
                'poke-gold': '#E6BC2F',
                // Colores de tipos (opcional para badges profesionales)
                'type-fire': '#F08030',
                'type-water': '#6890F0',
                'type-grass': '#78C850',
            }
        },
    },
    plugins: [],
}