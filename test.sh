#!/bin/bash

echo '<details><summary>Expand to see formatting issues</summary>' >> pr-message.md
echo >> pr-message.md
echo '```json' >> pr-message.md
cat bin/format-report.json >> pr-message.md
echo '```' >> pr-message.md
echo >> pr-message.md
echo '</details>' >> pr-message.md