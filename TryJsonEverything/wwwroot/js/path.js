const pathEditorName = 'editor-path';
const dataEditorName = 'editor-data';

initializeEditor(dataEditorName);

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
const pathSample = '$..book[?(@.price<10)]';

const pathEditor = document.getElementById(pathEditorName);
var value = localStorage.getItem('path.path');
if (value) {
	pathEditor.value = value;
} else {
	pathEditor.value = pathSample;
}
pathEditor.onkeyup = () => localStorage.setItem('path.path', pathEditor.value);

const dataEditor = ace.edit(dataEditorName);
value = localStorage.getItem('path.data');
if (value) {
	dataEditor.setValue(value);
} else {
	dataEditor.setValue(JSON.stringify(dataSample, null, '\t'));
}
dataEditor.clearSelection();
dataEditor.getSession().on('change', () => localStorage.setItem('path.data', dataEditor.getValue()));

async function requestQuery(path, data) {
	const body = {
		path: path,
		data: data
	};

	const response = await fetch(`${baseUri}api/path-query`,
		{
			method: 'POST',
			body: JSON.stringify(body),
			headers: {
				'Content-Type': 'application/json'
			}
		});
	return await response.json();
}

function getMatchElement(matchItem) {
	return `<li>
	<span class="text-monospace">Value: ${JSON.stringify(matchItem.value)}</span>
	<div class="text-monospace">Location: ${matchItem.location}</div>
</li>
`;
}

async function query() {
	const outputElement = document.getElementById('output');
	outputElement.innerHTML = '';

	const path = pathEditor.value;
	const instance = getJsonFromEditor(dataEditor);

	const response = await requestQuery(path, instance);
	console.log(response);

	if (response.error) {
		outputElement.innerHTML = `<h3 class="result-error">Error: ${response.error}</h3>`;
	} else if (response.validationErrors) {
		outputElement.innerHTML = '<h3 class="result-error">The path contains a syntax error.</h3>';
	} else if (response.result.matches.length === 0) {
		outputElement.innerHTML = '<h3>No matches found</h3>';
	} else {
		outputElement.innerHTML = `<ol type="1" class="text-left">${response.result.matches.map(getMatchElement).join('')}</ol>`;
	}

	scrollToEnd();
}