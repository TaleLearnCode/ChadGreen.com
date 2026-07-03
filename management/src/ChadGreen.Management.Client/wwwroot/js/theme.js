window.managementTheme = (() => {
    const storageKey = "management.theme";
    const darkTheme = "dark";
    const lightTheme = "light";

    const systemTheme = () =>
        window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches ? darkTheme : lightTheme;

    const normalizeTheme = (theme) => theme === darkTheme ? darkTheme : lightTheme;

    const applyTheme = (theme) => {
        const normalized = normalizeTheme(theme);
        document.documentElement.setAttribute("data-theme", normalized);
        return normalized;
    };

    return {
        getPreferredTheme: () => {
            const stored = window.localStorage.getItem(storageKey);
            return applyTheme(stored ?? systemTheme());
        },
        setTheme: (theme) => {
            const applied = applyTheme(theme);
            window.localStorage.setItem(storageKey, applied);
        }
    };
})();
