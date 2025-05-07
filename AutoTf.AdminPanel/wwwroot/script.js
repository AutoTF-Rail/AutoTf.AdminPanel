let cpuChartInstance = null;
let memoryChartInstance = null;
let networkChartInstance = null;

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

    for (const container of containers.sort((a, b) => (a.externalHost || '').localeCompare(b.externalHost || ''))) {
        const item = document.createElement('li');
        item.className = 'container-item';

        const containerInfo = await fetch(`/api/docker/getById/${container.containerId}`);
        const containerBody = await containerInfo.json();

        const name = container.externalHost.replace('autotf-', '') || '(no name)';
        const info = document.createElement('div');
        info.className = 'container-info';
        info.innerHTML = `<div class="container-name">${name}</div>
                      <div class="container-state">State: ${containerBody.state}</div>`;
        info.onclick = () => alert(`ID: ${container.containerId}\n\nRecordId: ${container.recordId}`);

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
    }
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


// Stats

async function fetchDockerStats() {
    const res = await fetch('/api/docker/stats/');
    const stats = await res.json();

    const cpuDocker = +(stats.cpuUsage.toFixed(2));
    const cpuSystem = +(stats.systemStats.cpuUsagePercent.toFixed(2));
    const cpuOther = Math.max(cpuSystem - cpuDocker, 0);
    const cpuIdle = Math.max(100 - cpuSystem, 0);

    const cpuData = [cpuOther, cpuDocker, cpuIdle];
    const cpuColors = ['#ffc107', '#007bff', '#e0e0e0'];

    const memoryUsed = stats.memory.memoryUsageMb / 1024;
    const memoryTotal = stats.memory.memoryLimitMb / 1024;
    const memoryPercentage = +(stats.memory.memoryPercentage).toFixed(2);

    const netRecv = +(stats.network.totalReceived / 1024).toFixed(2);
    const netSend = +(stats.network.totalSend / 1024).toFixed(2);

    document.getElementById('cpuPercent').innerText = `${cpuDocker}%`;


    if (!cpuChartInstance) {
        cpuChartInstance = new Chart(document.getElementById('cpuChart'), {
            type: 'doughnut',
            data: {
                datasets: [{
                    data: cpuData,
                    backgroundColor: cpuColors,
                    borderWidth: 0
                }]
            },
            options: {
                cutout: '70%',
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: (ctx) => {
                                const labelMap = ['Other System CPU %', 'Docker CPU %', 'Idle CPU %'];
                                return `${labelMap[ctx.dataIndex]}: ${ctx.formattedValue}%`;
                            }
                        }
                    },
                    legend: { display: false }
                }
            }
        });
    } else {
        cpuChartInstance.data.datasets[0].data = cpuData;
        cpuChartInstance.update('none');
    }

    const dockerMemMb = stats.memory.memoryUsageMb;
    const systemUsedMb = stats.systemStats.usedMemoryMb;
    const systemTotalMb = stats.systemStats.totalMemoryMb;

    const otherMemMb = Math.max(systemUsedMb - dockerMemMb, 0);
    const freeMemMb = Math.max(systemTotalMb - systemUsedMb, 0);

    const memPercent = +(stats.memory.memoryPercentage).toFixed(2);
    document.getElementById('memoryPercent').innerText = `${memPercent}%`;
    document.getElementById('memoryStatTotal').innerText = `${(dockerMemMb / 1024).toFixed(2)}/${(systemTotalMb / 1024).toFixed(2)} GB`;

    const memoryData = [otherMemMb, dockerMemMb, freeMemMb];
    const memoryColors = ['#ffc107', '#28a745', '#e0e0e0'];

    if (!memoryChartInstance) {
        memoryChartInstance = new Chart(document.getElementById('memoryChart'), {
            type: 'doughnut',
            data: {
                datasets: [{
                    data: memoryData,
                    backgroundColor: memoryColors,
                    borderWidth: 0
                }]
            },
            options: {
                cutout: '70%',
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: (ctx) => {
                                const labelMap = ['Other System Memory', 'Docker Memory', 'Free Memory'];
                                return `${labelMap[ctx.dataIndex]}: ${(memoryData[ctx.dataIndex] / 1024).toFixed(2)} GB`;
                            }
                        }
                    },
                    legend: { display: false }
                }
            }
        });
    } else {
        memoryChartInstance.data.datasets[0].data = memoryData;
        memoryChartInstance.update('none');
    }

    if (!networkChartInstance) {
        networkChartInstance = new Chart(document.getElementById('networkChart'), {
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
    } else {
        networkChartInstance.data.datasets[0].data = [netRecv, netSend];
        networkChartInstance.update('none');
    }
}

Promise.all([
    fetchDockerStats(),
    fetchManaged(),
    fetchDocker(),
    fetchPlesk(),
    fetchAuthentik(),
    fetchCloudflare()
]).then(() => {
    invokeLoadingScreen(false);
    console.log("Initialization complete");
});

function invokeLoadingScreen(visible)
{
    if(visible === true)
        document.getElementById('loadingArea').classList.add('open');
    else
        document.getElementById('loadingArea').classList.remove('open');
}

setInterval(fetchDockerStats, 2500);