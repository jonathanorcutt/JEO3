window.awesomeTreeView = {
    init: function () {
        // reserved for future global setup (e.g. resize observers)
    },
    setIndeterminate: function (elements, states) {
        elements.forEach((el, i) => { if (el) el.indeterminate = !!states[i]; });
    }
};
