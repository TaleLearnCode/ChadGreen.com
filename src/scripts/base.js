let isDark = false;
(() => {
  'use strict';
  console.log('Hello world!')

  let theme = JSON.parse(localStorage.getItem('chad-green-theme') || 'false');
  if (!theme && window.matchMedia('(prefers-color-scheme: dark)')?.matches) {
    isDark = true
  }
  if (isDark) {
    document.querySelector('body').classList.add('dark');
  }
})()

function toggleTheme() {
  isDark = !isDark
  document.querySelector('body').classList.toggle('dark');
  localStorage.setItem('chad-green-theme', isDark.toString());
}