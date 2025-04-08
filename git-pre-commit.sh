#!/bin/sh

echo "Running pre-commit hook..."

dotnet msbuild "$(dirname "$0")/Directory.Build.props" -target:UpdateVersionFromGit -noLogo

if [ $? -ne 0 ]; then
    echo "Error: Failed to update version from Git. Commit aborted."
    exit 1
fi

git add Directory.Build.props

if [ $? -ne 0 ]; then
    echo "Error: Failed to stage Directory.Build.props. Commit aborted."
    exit 1
fi

# Success
exit 0
