const pathEditorName = 'editor-path';
const dataEditorName = 'editor-data';

initializeEditor(dataEditorName);

const pathEditor = document.getElementById(pathEditorName);
var cookie = Cookies.get('path.path');
if (cookie !== undefined) {
	pathEditor.value = cookie;
}
pathEditor.onkeyup = () => Cookies.set('path.path', pathEditor.value);

const dataEditor = ace.edit(dataEditorName);
cookie = Cookies.get('path.data');
if (cookie !== undefined) {
	dataEditor.setValue(cookie);
}
dataEditor.clearSelection();
dataEditor.getSession().on('change', () => Cookies.set('path.data', dataEditor.getValue()));

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
		return;
	}

	outputElement.innerHTML = `<ol type="1" class="text-left">${response.result.matches.map(getMatchElement).join('')}</ol>`;
}