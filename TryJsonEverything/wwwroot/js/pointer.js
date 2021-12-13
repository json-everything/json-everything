const pointerEditorName = 'editor-pointer';
const dataEditorName = 'editor-data';
const outputEditorName = 'editor-output';

initializeEditor(dataEditorName);
initializeEditor(outputEditorName);

const dataSample = {
	"store": {
		"book": [
			{
				"category": "reference",
				"author": "Nigel Rees",
				"title": "Sayings of the Century",
				"price": 8.95
			},
			{
				"category": "fiction",
				"author": "Evelyn Waugh",
				"title": "Sword of Honour",
				"price": 12.99
			},
			{
				"category": "fiction",
				"author": "Herman Melville",
				"title": "Moby Dick",
				"isbn": "0-553-21311-3",
				"price": 8.99
			},
			{
				"category": "fiction",
				"author": "J. R. R. Tolkien",
				"title": "The Lord of the Rings",
				"isbn": "0-395-19395-8",
				"price": 22.99
			}
		],
		"bicycle": {
			"color": "red",
			"price": 19.95
		}
	}
};
const pointerSample = '/store/book/1/title';

const pointerEditor = document.getElementById(pointerEditorName);
var value = localStorage.getItem('pointer.pointer');
if (value) {
	pointerEditor.value = value;
} else {
	pointerEditor.value = pointerSample;
}
pointerEditor.onkeyup = () => localStorage.setItem('pointer.pointer', pointerEditor.value);

const dataEditor = ace.edit(dataEditorName);
value = localStorage.getItem('pointer.data');
if (value) {
	dataEditor.setValue(value);
} else {
	dataEditor.setValue(JSON.stringify(dataSample, null, '\t'));
}
dataEditor.clearSelection();
dataEditor.getSession().on('change', () => localStorage.setItem('pointer.data', dataEditor.getValue()));

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
	} else if (response.result !== undefined) {
		outputEditor.setValue(JSON.stringify(response.result, null, '\t'));
		outputEditor.clearSelection();
	}

	scrollToEnd();
}