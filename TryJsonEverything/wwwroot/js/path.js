const pathEditorName = 'editor-path';
const dataEditorName = 'editor-data';

initializeEditor(dataEditorName);

async function requestQuery(path, data) {
	const body = {
		path: path,
		data: data
	};

	const response = await fetch('https://localhost:5001/api/path-query',
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
	outputElement.innerHTML = "";

	const pathEditor = document.getElementById(pathEditorName);
	const dataEditor = ace.edit(dataEditorName);

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