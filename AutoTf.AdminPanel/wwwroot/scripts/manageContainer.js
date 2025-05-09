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


    document.getElementById('statEvu').innerHTML = container.EvuName;
    document.getElementById('statUrl').innerHTML = container.SubDomain;

    const allowedTrainsCountRes = await fetch(`/api/docker/${container.containerId}/allowedTrainsCount`);
    const allowedTrainsCount = await allowedTrainsCountRes.json();

    document.getElementById('manageAllowedTrains').innerHTML = allowedTrainsCount;

    const trainCountRes = await fetch(`/api/docker/${container.containerId}/trainCount`);
    const trainCount = await trainCountRes.json();

    document.getElementById('statTrains').innerHTML = trainCount;


    const sizeRes = await fetch(`/api/docker/${container.containerId}/size`);
    const size = await sizeRes.json();

    document.getElementById('statStorage').innerHTML = `${size} GB`;
    

    const containerInfo = await fetch(`/api/docker/getById/${container.containerId}`);
    const containerBody = await containerInfo.json();
    
    const state = containerBody.state;
    
    document.getElementById('statStatus').innerHTML = state;


    startButton = document.getElementById('startContainerButton');
    stopButton = document.getElementById('stopContainerButton');

    if (state === "running") {
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