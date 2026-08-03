(function () {
  try {
    var storedTheme = localStorage.getItem('smart-task-theme');
    var theme = storedTheme === 'classic' || storedTheme === 'field-ledger'
      ? storedTheme
      : 'field-ledger';
    document.documentElement.setAttribute('data-theme', theme);
    document.documentElement.style.colorScheme = 'light';
  } catch (_) {
    document.documentElement.setAttribute('data-theme', 'field-ledger');
  }
})();
