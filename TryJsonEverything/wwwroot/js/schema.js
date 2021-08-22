function initializeEditor(name) {
	var editor = ace.edit(name);
	editor.setTheme("ace/theme/monokai");
	editor.session.setMode("ace/mode/json");
	editor.session.setOptions({
		theme: "ace/theme/monokai",
		mode: "ace/mode/json",
		tabSize: 2
	});
}

initializeEditor("editor-schema");
initializeEditor("editor-instance");