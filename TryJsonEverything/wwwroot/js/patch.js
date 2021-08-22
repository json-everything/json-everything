const patchEditorName = 'editor-patch';
const dataEditorName = 'editor-data';
const outputEditorName = 'editor-output';

initializeEditor(patchEditorName);
initializeEditor(dataEditorName);
initializeEditor(outputEditorName);

var schemaEditor = ace.edit(outputEditorName);
schemaEditor.setReadOnly(true);

async function requestApplication(patch, data) {
	const body = {
		patch: patch,
		data: data
	};

	const response = await fetch('https://localhost:5001/api/patch-apply',
		{
			method: 'POST',
			body: JSON.stringify(body),
			headers: {
				'Content-Type': 'application/json'
			}
		});
	return await response.json();
}

async function apply() {
	const outputElement = document.getElementById("error-output");
	outputElement.innerHTML = "";

	const patchEditor = ace.edit(patchEditorName);
	const dataEditor = ace.edit(dataEditorName);
	const outputEditor = ace.edit(outputEditorName);

	const patch = getJsonFromEditor(patchEditor);
	const data = getJsonFromEditor(dataEditor);

	const response = await requestApplication(patch, data);

	if (response.result.isSuccess) {
		outputElement.innerHTML = '<h3 class="result-valid">Patch applied successfully!</h3>';
	} else {
		outputElement.innerHTML = `<h3 class="result-error">Error: ${response.result.error}</h3><p>Halted after ${response.result.operation} operations.</p>`;
	}

	if (response.result.result) {
		outputEditor.setValue(JSON.stringify(response.result.result, null, '\t'));
		outputEditor.clearSelection();
	}
}