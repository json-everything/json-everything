const patchEditorName = 'editor-patch';
const dataEditorName = 'editor-data';
const outputEditorName = 'editor-output';

initializeEditor(patchEditorName);
initializeEditor(dataEditorName);
initializeEditor(outputEditorName);

const patchEditor = ace.edit(patchEditorName);
var cookie = Cookies.get('patch.patch');
if (cookie !== undefined) {
	patchEditor.setValue(cookie);
}
patchEditor.clearSelection();
patchEditor.getSession().on('change', () => Cookies.set('patch.patch', patchEditor.getValue()));

const dataEditor = ace.edit(dataEditorName);
cookie = Cookies.get('patch.data');
if (cookie !== undefined) {
	dataEditor.setValue(cookie);
}
dataEditor.clearSelection();
dataEditor.getSession().on('change', () => Cookies.set('patch.data', dataEditor.getValue()));

const outputEditor = ace.edit(outputEditorName);
outputEditor.setReadOnly(true);

async function requestApplication(patch, data) {
	const body = {
		patch: patch,
		data: data
	};

	const response = await fetch(`${baseUri}api/patch-apply`,
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

	outputEditor.setValue('');

	const patch = getJsonFromEditor(patchEditor);
	const data = getJsonFromEditor(dataEditor);

	const response = await requestApplication(patch, data);
	console.log(response);

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