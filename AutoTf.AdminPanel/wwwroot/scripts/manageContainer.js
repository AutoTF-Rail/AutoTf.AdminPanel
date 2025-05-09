let _container = null

let startButton = null;
let stopButton = null;

async function openManageDialog(container) {
    invokeLoadingScreen(true);
    console.log("Opening manage dialog for: ", containerId);
    
    _container = container;

    document.getElementById('statEvu').innerHTML = _container.evuName;
    document.getElementById('statUrl').innerHTML = _container.subDomain;

    const allowedTrainsCountRes = await fetch(`/api/docker/${_container.containerId}/allowedTrainsCount`);
    const allowedTrainsCount = await allowedTrainsCountRes.json();

    document.getElementById('manageAllowedTrains').value = allowedTrainsCount;

    const trainCountRes = await fetch(`/api/docker/${_container.containerId}/trainCount`);
    const trainCount = await trainCountRes.json();

    document.getElementById('statTrains').innerHTML = trainCount;


    const sizeRes = await fetch(`/api/docker/${_container.containerId}/size`);
    const size = await sizeRes.json();

    document.getElementById('statStorage').innerHTML = `${size} GB`;
    

    const containerInfo = await fetch(`/api/docker/getById/${_container.containerId}`);
    const containerBody = await containerInfo.json();
    
    const state = containerBody.state;
    
    document.getElementById('statStatus').innerHTML = state;


    startButton = document.getElementById('startContainerButton');
    stopButton = document.getElementById('stopContainerButton');

    if (state === "running") {
        stopButton.style.visibility = "visible";
        startButton.style.visibility = "collapsed";
    }
    else
    {
        stopButton.style.visibility = "hidden";
        startButton.style.visibility = "collapsed";
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

async function deleteContainer() {
    invokeLoadingScreen(true);
    if (confirm(`Are you sure you want to delete this container?`)) {
        await fetch(`/api/manage/${_container.id}`, { method: 'DELETE' });
        await fetchManaged();
        closeManageDialog();
    }
    invokeLoadingScreen(false);
}