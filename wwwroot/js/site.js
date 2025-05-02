import Alpine from "alpinejs";
import flatpickr from "flatpickr";
import "flatpickr/dist/flatpickr.css";
import focus from '@alpinejs/focus'
import mask from '@alpinejs/mask'
import intersect from '@alpinejs/intersect'
import anchor from '@alpinejs/anchor'

window.Alpine = Alpine;
window.Alpine.plugin(focus);
window.Alpine.plugin(mask);
window.Alpine.plugin(intersect);
window.Alpine.plugin(anchor);

window.Alpine.start();

