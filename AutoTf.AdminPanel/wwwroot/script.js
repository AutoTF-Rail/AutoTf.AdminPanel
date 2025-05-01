function toggleSection(id) {
    const content = document.getElementById(id);
    content.style.display = content.style.display === 'block' ? 'none' : 'block';
}

// ---- Docker ----
async function fetchDocker() {
    const res = await fetch('/api/docker/getAllContainers');
    const containers = await res.json();
    const list = document.getElementById('dockerContent');
    list.innerHTML = '';
    containers.forEach(container => {
        const item = document.createElement('li');
        item.className = 'container-item';

        const name = container.names?.[0]?.replace(/^\//, '') || '(no name)';
        const info = document.createElement('div');
        info.className = 'container-info';
        info.innerHTML = `<div class="container-name">${name}</div>
                      <div class="container-state">State: ${container.state}</div>`;
        info.onclick = () => alert(`ID: ${container.id}\nImage: ${container.image}\nCommand: ${container.command}`);

        const hidden = document.createElement('input');
        hidden.type = 'hidden';
        hidden.value = container.id;

        const del = document.createElement('button');
        del.className = 'delete-btn';
        del.textContent = 'Delete';
        del.onclick = async () => {
            await fetch(`/api/docker/deleteContainer/${container.id}`, { method: 'DELETE' });
            fetchDocker();
        };

        item.append(hidden, info, del);
        list.appendChild(item);
    });
}

// ---- Plesk ----
async function fetchPlesk() {
    const res = await fetch('/api/plesk/all');
    const domains = await res.json();
    const list = document.getElementById('pleskContent');
    list.innerHTML = '';
    domains.forEach(domain => {
        const item = document.createElement('li');
        item.className = 'container-item';

        const info = document.createElement('div');
        info.className = 'container-info';
        info.innerHTML = `<div class="container-name">${domain}</div>`;

        const del = document.createElement('button');
        del.className = 'delete-btn';
        del.textContent = 'Delete';
        del.onclick = async () => {
            await fetch(`/api/plesk/delete/${domain}`, { method: 'DELETE' });
            fetchPlesk();
        };

        item.appendChild(info);
        item.appendChild(del);
        list.appendChild(item);
    });
}

// ---- Authentik ----
async function fetchAuthentik() {
    const res = await fetch('/api/authentik/providers/');
    const data = await res.json();
    const list = document.getElementById('authentikContent');
    list.innerHTML = '';
    data.results.forEach(provider => {
        const item = document.createElement('li');
        item.className = 'container-item';

        const name = provider.name.replace(/^Managed provider for\s*/i, '');
        const info = document.createElement('div');
        info.className = 'container-info';
        info.innerHTML = `<div class="container-name">${name}</div>
                      <div class="container-state">External: ${provider.external_host}</div>`;

        const hidden = document.createElement('input');
        hidden.type = 'hidden';
        hidden.value = provider.pk;

        const del = document.createElement('button');
        del.className = 'delete-btn';
        del.textContent = 'Delete';
        del.onclick = async () => {
            await fetch(`/api/authentik/delete/${provider.pk}`, { method: 'DELETE' });
            fetchAuthentik();
        };

        item.append(hidden, info, del);
        list.appendChild(item);
    });
}

// ---- Cloudflare ----
async function fetchCloudflare() {
    const res = await fetch('/api/cloudflare/all');
    const records = await res.json();
    const list = document.getElementById('cloudflareContent');
    list.innerHTML = '';
    records.forEach(record => {
        const item = document.createElement('li');
        item.className = 'container-item';

        const info = document.createElement('div');
        info.className = 'container-info';
        info.innerHTML = `<div class="container-name">[${record.type}] ${record.name}</div>
                      <div class="container-state">→ ${record.content} ${record.proxied ? '(Proxied)' : ''}</div>`;

        const hidden = document.createElement('input');
        hidden.type = 'hidden';
        hidden.value = record.id;

        const del = document.createElement('button');
        del.className = 'delete-btn';
        del.textContent = 'Delete';
        del.onclick = async () => {
            await fetch(`/api/cloudflare/delete/${record.id}`, { method: 'DELETE' });
            fetchCloudflare();
        };

        item.append(hidden, info, del);
        list.appendChild(item);
    });
}

fetchDocker();
fetchPlesk();
fetchAuthentik();
fetchCloudflare();