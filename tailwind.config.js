/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
      "./Views/**/*.cshtml",
      "./Views/Home/**/*.cshtml",
      "./Views/Auth/**/*.cshtml",
      "./Views/Admin/Partials/*.cshtml",
      "./Views/Organizer/Partials/*.cshtml",
      "./Views/Attendee/Partials/*.cshtml",
  ],
  theme: {
      extend: {
          fontFamily: {
              poppins: ["Poppins", "sans-serif"],
          },

          fontSize: {
              "3xs": ["0.5rem", "0.5rem"],
              "2xs": ["0.625rem", "0.75rem"],
          },
          colors: {
              charcoal: {
                  50: "#F5F5F4",
                  100: "#E2E0DF",
                  200: "#D9D5D4",
                  300: "#C5C1BE",
                  400: "#A9A19E",
                  500: "#8C827D",
                  600: "#6C6460",
                  700: "#56504D",
                  800: "#413C3A",
                  900: "#2B2826",
                  950: "#191716",
              },
              coffee: {
                  50: "#F8F3F2",
                  100: "#F1E7E4",
                  200: "#E2D0CA",
                  300: "#CDACA2",
                  400: "#BF9588",
                  500: "#B07D6D",
                  600: "#8D5B4C",
                  700: "#6A4539",
                  800: "#50332B",
                  900: "#35221D",
                  950: "#1B110E",
              },
              carribean: {
                  50: "#F1F8F8",
                  100: "#C8E1E4",
                  200: "#ADD2D7",
                  300: "#92C4C9",
                  400: "#76B5BC",
                  500: "#5BA6AE",
                  600: "#4A8F96",
                  700: "#3A6F75",
                  800: "#284E52",
                  900: "#1B3437",
                  950: "#0D1A1B",
              },
              pumpkin: {
                  50: "#FFF2EB",
                  100: "#FFD9C2",
                  200: "#FFC099",
                  300: "#FFA770",
                  400: "#FF8C42",
                  500: "#FF680A",
                  600: "#E05600",
                  700: "#B84600",
                  800: "#8F3700",
                  900: "#662700",
                  950: "#291000",
              },
          },
      },
  },
  plugins: [require("tailwind-scrollbar")],
};

