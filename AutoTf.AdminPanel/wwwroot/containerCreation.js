function openCreateDialog() {
    const dialog = document.getElementById('createDialog');
    dialog.classList.add('open');

    fetch('/api/system/config')
        .then(response => response.json())
        .then(config => {
            document.getElementById('dnsContent').value = config.defaultTarget;
            document.getElementById('proxied').value = config.defaultProxySetting ? 'true' : 'false';
            document.getElementById('ttl').value = config.defaultTtl;
            document.getElementById('dnsComment').value = '';

            document.getElementById('imageField').value = config.defaultImage;
            document.getElementById('pleskEmail').value = config.defaultCertificateEmail;
            document.getElementById('authentikHost').value = config.defaultAuthentikHost;

            fetch('/api/docker/networks')
                .then(res => res.json())
                .then(networks => {
                    const defaultNet = document.getElementById('defaultNetwork');
                    const additionalNet = document.getElementById('additionalNetwork');
                    defaultNet.innerHTML = '';
                    additionalNet.innerHTML = '';

                    networks.forEach(n => {
                        const isDefault = n === config.defaultNetwork;
                        const isAdditional = n === config.defaultAdditionalNetwork;
                        defaultNet.innerHTML += `<option value="${n}" ${isDefault ? 'selected' : ''}>${n}</option>`;
                        additionalNet.innerHTML += `<option value="${n}" ${isAdditional ? 'selected' : ''}>${n}</option>`;
                    });
                });

            fetch('/api/authentik/flows/authorization')
                .then(res => res.json())
                .then(flows => {
                    const authFlow = document.getElementById('authFlow');
                    authFlow.innerHTML = '';
                    flows.forEach(flow => {
                        const selected = flow.pk === config.defaultAuthorizationFlow ? 'selected' : '';
                        authFlow.innerHTML += `<option value="${flow.pk}" ${selected}>${flow.slug}</option>`;
                    });
                });

            fetch('/api/authentik/flows/invalidation')
                .then(res => res.json())
                .then(flows => {
                    const invalidationFlow = document.getElementById('invalidationFlow');
                    invalidationFlow.innerHTML = '';
                    flows.forEach(flow => {
                        const selected = flow.pk === config.defaultInvalidationFlow ? 'selected' : '';
                        invalidationFlow.innerHTML += `<option value="${flow.pk}" ${selected}>${flow.slug}</option>`;
                    });
                });

            fetch('/api/authentik/groups')
                .then(res => res.json())
                .then(groups => {
                    const groupSelect = document.getElementById('allowedGroup');
                    groupSelect.innerHTML = '';
                    groups.forEach(group => {
                        const selected = group.pk === config.defaultAllowedGroup ? 'selected' : '';
                        groupSelect.innerHTML += `<option value="${group.pk}" ${selected}>${group.name}</option>`;
                    });
                });

            fetch('/api/authentik/outposts')
                .then(res => res.json())
                .then(data => {
                    const outpost = document.getElementById('outpost');
                    outpost.innerHTML = '';
                    data.results.forEach(item => {
                        const selected = item.pk === config.defaultOutpost ? 'selected' : '';
                        outpost.innerHTML += `<option value="${item.pk}" ${selected}>${item.name}</option>`;
                    });
                });

        })
        .catch(error => {
            console.error('Failed to load default config:', error);
        });
}

function toggleCollapse(id) {
    const el = document.getElementById(id);
    el.classList.toggle('show');
}