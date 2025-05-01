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

            const idSpan = document.createElement('span');
            idSpan.className = 'container-id';
            idSpan.textContent = container.ID;

            idSpan.addEventListener('click', () => {
                alert(`Container ID: ${container.ID}\nWarnings: ${container.Warnings?.join(', ') || 'None'}`);
            });

            const deleteBtn = document.createElement('button');
            deleteBtn.className = 'delete-btn';
            deleteBtn.textContent = 'Delete';
            deleteBtn.onclick = async () => {
                await fetch(`/api/docker/deleteContainer/${container.ID}`, {
                    method: 'DELETE',
                });
                fetchContainers();
            };

            li.appendChild(idSpan);
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

    // Initial fetch
    fetchContainers();
});