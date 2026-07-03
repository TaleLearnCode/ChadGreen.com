(() => {
    const editors = new Map();

    const baseToolbar = [
        "bold",
        "italic",
        "heading",
        "|",
        "quote",
        "unordered-list",
        "ordered-list",
        "|",
        "link",
        "image",
        "table",
        "|",
        "preview",
        "side-by-side",
        "guide"
    ];

    const getEntry = (id) => editors.get(id);

    function initialize(id, dotNetRef, placeholder) {
        const target = document.getElementById(id);
        if (!target) {
            return false;
        }

        dispose(id);

        if (typeof window.EasyMDE === "undefined") {
            return false;
        }

        const easyMde = new window.EasyMDE({
            element: target,
            autoDownloadFontAwesome: false,
            forceSync: true,
            spellChecker: false,
            status: false,
            minHeight: "260px",
            toolbar: baseToolbar,
            placeholder: placeholder ?? "",
            renderingConfig: {
                singleLineBreaks: false,
                codeSyntaxHighlighting: true
            }
        });

        const changeHandler = () => {
            dotNetRef.invokeMethodAsync("NotifyMarkdownChanged", easyMde.value()).catch(() => {});
        };

        const blurHandler = () => {
            dotNetRef.invokeMethodAsync("NotifyEditorBlur").catch(() => {});
        };

        const cm = easyMde.codemirror;
        cm.on("change", changeHandler);
        cm.on("blur", blurHandler);

        editors.set(id, { easyMde, dotNetRef, changeHandler, blurHandler });
        return true;
    }

    function setValue(id, value) {
        const entry = getEntry(id);
        const normalized = value ?? "";

        if (!entry) {
            const target = document.getElementById(id);
            if (target && target.value !== normalized) {
                target.value = normalized;
            }
            return;
        }

        if (entry.easyMde.value() === normalized) {
            return;
        }

        const doc = entry.easyMde.codemirror.getDoc();
        const cursor = doc.getCursor();
        entry.easyMde.value(normalized);
        doc.setCursor(cursor);
    }

    function dispose(id) {
        const entry = getEntry(id);
        if (!entry) {
            return;
        }

        try {
            entry.easyMde.toTextArea();
        } catch {
            // no-op
        }

        editors.delete(id);
    }

    window.managementMarkdownEditor = {
        initialize,
        setValue,
        dispose
    };
})();
