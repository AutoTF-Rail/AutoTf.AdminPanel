function openManageDialog(containerId) {
    const container = containers.find(c => c.id === containerId);
    if (!container) return;

    document.getElementById('manageContainerName').textContent = container.name || containerId;
    document.getElementById('manageId').value = container.id;
    document.getElementById('manageStatus').value = container.status;
    document.getElementById('manageNetwork').value = container.network;
    document.getElementById('manageImage').value = container.image;
    document.getElementById('manageTrains').value = container.allowedTrains;

    document.getElementById('manageDialog').classList.add('open');
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