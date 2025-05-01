document.addEventListener('DOMContentLoaded', () => {
    const listEl = document.getElementById('containerList');
    const refreshBtn = document.getElementById('refreshBtn');
    const createBtn = document.getElementById('createBtn');

    async function fetchContainers() {
        const res = await fetch('/api/docker/getAllContainers');
        const containers = await res.json();
        renderContainers(containers);
    }

    function renderContainers(containers) {
        listEl.innerHTML = '';
        containers.forEach(container => {
            const li = document.createElement('li');
            li.className = 'container-item';

            const idInput = document.createElement('input');
            idInput.type = 'hidden';
            idInput.value = container.id;

            const name = (container.names && container.names.length > 0)
                ? container.names[0].replace(/^\//, '')
                : '(no name)';

            const infoDiv = document.createElement('div');
            infoDiv.className = 'container-info';

            const nameEl = document.createElement('div');
            nameEl.className = 'container-name';
            nameEl.textContent = name;

            const stateEl = document.createElement('div');
            stateEl.className = 'container-state';
            stateEl.textContent = `State: ${container.state}`;

            infoDiv.appendChild(nameEl);
            infoDiv.appendChild(stateEl);

            infoDiv.addEventListener('click', () => {
                alert(`Name: ${name}
                    ID: ${container.id}
                    Image: ${container.image}
                    Command: ${container.command}
                    Status: ${container.status}`);
            });

            const deleteBtn = document.createElement('button');
            deleteBtn.className = 'delete-btn';
            deleteBtn.textContent = 'Delete';
            deleteBtn.onclick = async () => {
                await fetch(`/api/docker/deleteContainer/${container.id}`, {
                    method: 'DELETE',
                });
                fetchContainers();
            };

            li.appendChild(idInput);
            li.appendChild(infoDiv);
            li.appendChild(deleteBtn);
            listEl.appendChild(li);
        });
    }

    refreshBtn.addEventListener('click', fetchContainers);

    createBtn.addEventListener('click', async () => {
        await fetch('/api/docker/createContainer', {
            method: 'POST'
        });
        fetchContainers();
    });

    fetchContainers();
});