let _container = null

let startButton = null;
let stopButton = null;


async function saveAllowedTrains() {
    const newLimit = parseInt(document.getElementById("manageAllowedTrains").value, 10);
    
    if (isNaN(newLimit)) {
        alert("Please enter a valid number.");
        return;
    }

    try {
        const response = await fetch(`https://admin.autotf.de/api/docker/${_container.containerId}/updateAllowedTrains`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(newLimit)
        });

        if (!response.ok) {
            throw new Error("Failed to update allowed trains");
        }
        
        console.log("Successfully updated the train count.")
        document.getElementById("manageInfoField").innerHTML = 'Successfully updated the train count.';
    } catch (error) {
        console.error(error);
        document.getElementById("manageInfoField").innerHTML = 'Failed to update the train count.';
    }
}

async function openManageDialog(container) {
    invokeLoadingScreen(true);
    console.log("Opening manage dialog for: ", container.containerId);
    
    _container = container;

    document.getElementById("manageInfoField").innerHTML = '';
    
    
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
        startButton.style.visibility = "collapse";
    }
    else
    {
        stopButton.style.visibility = "hidden";
        startButton.style.visibility = "collapse";
    }
    
    document.getElementById('manageDialog').classList.add('open');
    invokeLoadingScreen(false);
}

async function closeManageDialog() {
    document.getElementById('manageDialog').classList.remove('open');
    await fetchManaged();
    await fetchOtherStats();
    
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
        await closeManageDialog();
    }
    invokeLoadingScreen(false);
}