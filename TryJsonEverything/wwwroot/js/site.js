function initializeEditor(name) {
	const editor = ace.edit(name);
	editor.setTheme('ace/theme/monokai');
	editor.session.setMode('ace/mode/json');
	editor.session.setTabSize(2);
	editor.setShowPrintMargin(false);
}

function getJsonFromEditor(editor) {
	const annotations = editor.getSession().getAnnotations();
	if (annotations.length !== 0) {
		return null;
	}
	const text = editor.getValue();
	return JSON.parse(text);
}

$(function () {
	$('[data-toggle="tooltip"]').tooltip();
});