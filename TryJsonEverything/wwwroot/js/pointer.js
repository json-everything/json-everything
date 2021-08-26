const pointerEditorName = 'editor-pointer';
const dataEditorName = 'editor-data';
const outputEditorName = 'editor-output';

initializeEditor(dataEditorName);
initializeEditor(outputEditorName);

const pointerEditor = document.getElementById(pointerEditorName);
var cookie = Cookies.get('pointer.pointer');
if (cookie !== undefined) {
	pointerEditor.value = cookie;
}
pointerEditor.onkeyup = () => Cookies.set('pointer.pointer', pointerEditor.value);

const dataEditor = ace.edit(dataEditorName);
cookie = Cookies.get('pointer.data');
if (cookie !== undefined) {
	dataEditor.setValue(cookie);
}
dataEditor.clearSelection();
dataEditor.getSession().on('change', () => Cookies.set('pointer.data', dataEditor.getValue()));

const outputEditor = ace.edit(outputEditorName);

async function requestQuery(pointer, data) {
	const body = {
		pointer: pointer,
		data: data
	};

	const response = await fetch(`${baseUri}api/pointer-query`,
		{
			method: 'POST',
			body: JSON.stringify(body),
			headers: {
				'Content-Type': 'application/json'
			}
		});
	return await response.json();
}

async function query() {
	const outputElement = document.getElementById('error-output');
	outputElement.innerHTML = "";

	const pointerEditor = document.getElementById(pointerEditorName);
	const dataEditor = ace.edit(dataEditorName);
	const outputEditor = ace.edit(outputEditorName);
	outputEditor.setValue('');

	const pointer = pointerEditor.value;
	const instance = getJsonFromEditor(dataEditor);

	const response = await requestQuery(pointer, instance);
	console.log(response);

	if (response.errors) {
		outputElement.innerHTML = `<ol type="1" class="result-error text-left">${response.errors.map(getErrorElement).join('')}</ol>`;
		return;
	}

	if (response.result) {
		outputEditor.setValue(JSON.stringify(response.result, null, '\t'));
		outputEditor.clearSelection();
	}
}