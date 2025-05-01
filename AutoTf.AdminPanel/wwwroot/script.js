function toggleSection(id) {
    const content = document.getElementById(id);
    content.style.display = content.style.display === 'block' ? 'none' : 'block';
}

// ---- Managed ----
async function fetchManaged() {
    const res = await fetch('/api/manage/all');
    const containers = await res.json();
    const list = document.getElementById('managedContent');

    list.innerHTML = '';

    containers.sort((a, b) => (a.names?.[0] || '').localeCompare(b.names?.[0] || '')).forEach(container => {
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
            fetchManaged();
        };

        item.append(hidden, info, del);
        list.appendChild(item);
    });
}


// ---- Docker ----
async function fetchDocker() {
    const res = await fetch('/api/docker/getAllContainers');
    const containers = await res.json();
    const list = document.getElementById('dockerContent');

    list.innerHTML = '';

    containers.sort((a, b) => (a.names?.[0] || '').localeCompare(b.names?.[0] || '')).forEach(container => {
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
    
    domains.sort((a, b) => a.localeCompare(b)).forEach(domain => {
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
    data.results.sort((a, b) => {
        const nameA = a.name.replace(/^Managed provider for\s*/i, '');
        const nameB = b.name.replace(/^Managed provider for\s*/i, '');
        return nameA.localeCompare(nameB);
    }).forEach(provider => {
        
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
    
    records.sort((a, b) => a.name.localeCompare(b.name)).forEach(record => {
        
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

fetchManaged();
fetchDocker();
fetchPlesk();
fetchAuthentik();
fetchCloudflare();
toggleSection('managedContent');

// Stats

async function fetchDockerStats() {
    const res = await fetch('/api/docker/stats/');
    const stats = await res.json();

    const cpu = +stats.cpuUsage.toFixed(2);

    const memoryUsed = stats.memory.memoryUsageMb / 1024;
    const memoryTotal = stats.memory.memoryLimitMb / 1024;
    const memoryPercentage = +(stats.memory.memoryPercentage).toFixed(2);

    const netRecv = +(stats.network.totalReceived / 1024).toFixed(2); 
    const netSend = +(stats.network.totalSend / 1024).toFixed(2);
    
    document.getElementById('cpuPercent').innerText = `${cpu}%`;

    new Chart(document.getElementById('cpuChart'), {
        type: 'doughnut',
        data: {
            datasets: [{
                data: [cpu, 100 - cpu],
                backgroundColor: ['#007bff', '#e0e0e0'],
                borderWidth: 0
            }]
        },
        options: {
            cutout: '70%',
            plugins: {
                tooltip: { enabled: false },
                legend: { display: false }
            }
        }
    });

    document.getElementById('memoryPercent').innerText = `${memoryPercentage}%`;
    document.getElementById('memoryStatTotal').innerText = `${memoryUsed.toFixed(2)}/${memoryTotal.toFixed(2)} GB`;

    new Chart(document.getElementById('memoryChart'), {
        type: 'doughnut',
        data: {
            datasets: [{
                data: [memoryPercentage, 100 - memoryPercentage],
                backgroundColor: ['#28a745', '#e0e0e0'],
                borderWidth: 0
            }]
        },
        options: {
            cutout: '70%',
            plugins: {
                tooltip: { enabled: false },
                legend: { display: false }
            }
        }
    });

    new Chart(document.getElementById('networkChart'), {
        type: 'bar',
        data: {
            labels: ['Received', 'Transmitted'],
            datasets: [{
                data: [ netRecv, netSend ],
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)',
                    'rgba(255, 159, 64, 0.2)'
                ],
                borderColor: [
                    'rgb(255, 99, 132)',
                    'rgb(255, 159, 64)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            plugins: {
                legend: { display: false }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: val => `${val} MB`
                    }
                }
            }
        }
    });
}

fetchDockerStats();