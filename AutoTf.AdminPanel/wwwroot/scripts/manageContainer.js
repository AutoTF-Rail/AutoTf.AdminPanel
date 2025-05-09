let _containerId = null

let startButton = null;
let stopButton = null;

async function openManageDialog(containerId) {
    invokeLoadingScreen(true);
    console.log("Opening manage dialog for: ", containerId);
    
    _containerId = containerId;
    const container = containers.find(c => c.containerId === containerId);
    if (!container) {
        invokeLoadingScreen(false);
        console.log("Could not find container by the given ID.")
        return;
    }


    document.getElementById('statEvu').value = container.evuName;
    document.getElementById('statUrl').value = container.subDomain;

    const allowedTrainsCountRes = await fetch(`/api/docker/${container.containerId}/allowedTrainsCount`);
    const allowedTrainsCount = await allowedTrainsCountRes.json();

    document.getElementById('manageAllowedTrains').value = allowedTrainsCount;

    const trainCountRes = await fetch(`/api/docker/${container.containerId}/trainCount`);
    const trainCount = await trainCountRes.json();

    document.getElementById('statTrains').value = trainCount


    const sizeRes = await fetch(`/api/docker/${container.containerId}/size`);
    const size = await sizeRes.json();

    document.getElementById('statStorage').value = size

    
    document.getElementById('statStatus').value = container.state


    startButton = document.getElementById('startContainerButton');
    stopButton = document.getElementById('stopContainerButton');

    if (container.state === "running") {
        stopButton.style.visibility = "visible";
        startButton.style.visibility = "hidden";
    }
    else
    {
        stopButton.style.visibility = "hidden";
        startButton.style.visibility = "visible";
    }
        // manageBtn.onclick = async () => {
        //     invokeLoadingScreen(true);
        //     // if (confirm(`Are you sure you want to delete "${container.externalHost.replace('autotf-', '') || '(no name)'}"?`)) {
        //     //     await fetch(`/api/manage/${container.id}`, { method: 'DELETE' });
        //     //     await fetchManaged();
        //     // }
        //     openManageDialog();
        //     invokeLoadingScreen(false);
        // };
    
    document.getElementById('manageDialog').classList.add('open');
    invokeLoadingScreen(false);
}

function closeManageDialog() {
    document.getElementById('manageDialog').classList.remove('open');
}

function changeTrainLimit(delta) {
    const input = document.getElementById('manageTrains');
    input.value = Math.max(0, parseInt(input.value || '0') + delta);
}

// Placeholder actions 
function startContainer() { alert("Starting..."); }
function stopContainer() { alert("Stopping..."); }
function restartContainer() { alert("Restarting..."); }
function updateContainer() { alert("Updating..."); }
function confirmDeleteContainer() {
    if (confirm("Are you sure you want to delete this container?")) {
        alert("Deleted.");
    }
}