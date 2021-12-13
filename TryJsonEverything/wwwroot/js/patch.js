const patchEditorName = 'editor-patch';
const dataEditorName = 'editor-data';
const outputEditorName = 'editor-output';

initializeEditor(patchEditorName);
initializeEditor(dataEditorName);
initializeEditor(outputEditorName);

const dataSample = {
	"baz": "qux",
	"foo": "bar"
};
const patchSample = [
	{ "op": "replace", "path": "/baz", "value": "boo" },
	{ "op": "add", "path": "/hello", "value": ["world"] },
	{ "op": "remove", "path": "/foo" }
];

const patchEditor = ace.edit(patchEditorName);
var value = localStorage.getItem('patch.patch');
if (value) {
	patchEditor.setValue(value);
} else {
	patchEditor.setValue(JSON.stringify(patchSample, null, '\t'));
}
patchEditor.clearSelection();
patchEditor.getSession().on('change', () => localStorage.setItem('patch.patch', patchEditor.getValue()));

const dataEditor = ace.edit(dataEditorName);
value = localStorage.getItem('patch.data');
if (value) {
	dataEditor.setValue(value);
} else {
	dataEditor.setValue(JSON.stringify(dataSample, null, '\t'));
}
dataEditor.clearSelection();
dataEditor.getSession().on('change', () => localStorage.setItem('patch.data', dataEditor.getValue()));

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

	scrollToEnd();
}

async function requestGeneration(start, target) {
	const body = {
		start: start,
		target: target
	};

	const response = await fetch(`${baseUri}api/patch-generate`,
		{
			method: 'POST',
			body: JSON.stringify(body),
			headers: {
				'Content-Type': 'application/json'
			}
		});
	return await response.json();
}

async function generate() {
	const outputElement = document.getElementById("error-output");
	outputElement.innerHTML = "";

	outputEditor.setValue('');

	const start = getJsonFromEditor(dataEditor);
	const target = getJsonFromEditor(patchEditor);

	const response = await requestGeneration(start, target);
	console.log(response);

	if (response.error) {
		outputElement.innerHTML = `<h3 class="result-error">Error: ${response.error}</h3>`;
	} else {
		outputElement.innerHTML = '<h3 class="result-valid">Patch generated successfully!</h3>';
	}

	if (response.patch !== undefined) {
		outputEditor.setValue(JSON.stringify(response.patch, null, '\t'));
		outputEditor.clearSelection();
	}

	scrollToEnd();
}