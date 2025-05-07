let systemConfig = null;

function openCreateDialog() {
    invokeLoadingScreen(true);
    const dialog = document.getElementById('createDialog');
    dialog.classList.add('open');

    fetch('/api/system/config')
        .then(response => response.json())
        .then(config => {
            systemConfig = config;
            
            document.getElementById('dnsContent').value = config.defaultTarget;
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
        .then(x => invokeLoadingScreen(false))
        .catch(error => {
            console.error('Failed to load default config:', error);
        });
    
}

function submitContainerCreation() {
    if (!systemConfig) {
        alert('System config not loaded.');
        return;
    }
    
    invokeLoadingScreen(true);

    const evuName = document.getElementById('evuName').value;
    const launchSubdomain = document.getElementById('launchSubdomain').value;
    const dnsComment = document.getElementById('dnsComment').value;

    const defaultNetwork = document.getElementById('defaultNetwork').value;
    const additionalNetwork = document.getElementById('additionalNetwork').value;
    const image = document.getElementById('imageField').value;

    const authFlow = document.getElementById('authFlow').value;
    const invalidationFlow = document.getElementById('invalidationFlow').value;
    const allowedGroup = document.getElementById('allowedGroup').value;
    const outpost = document.getElementById('outpost').value;

    const pleskEmail = document.getElementById('pleskEmail').value;
    const authentikHost = document.getElementById('authentikHost').value;

    const launchDomain = `https://${launchSubdomain}.autotf.de`;

    const payload = {
        DnsRecord: {
            Type: systemConfig.defaultDnsType,
            Name: launchSubdomain,
            Content: systemConfig.defaultTarget,
            Proxied: systemConfig.defaultProxySetting,
            Ttl: systemConfig.defaultTtl,
            Comment: dnsComment
        },
        Container: {
            DefaultNetwork: defaultNetwork,
            AdditionalNetwork: additionalNetwork,
            Image: image,
            EvuName: evuName,
            ContainerName: evuName
        },
        Proxy: {
            Name: evuName,
            LaunchUrl: launchDomain,
            AuthorizationFlow: authFlow,
            InvalidationFlow: invalidationFlow,
            ExternalHost: launchDomain,
            PolicyBindings: [
                {
                    group: allowedGroup,
                    negate: false,
                    enabled: true,
                    order: "0",
                    timeout: "30",
                    failure_result: false
                }
            ],
            OutpostId: outpost
        },
        Plesk: {
            subdomain: launchSubdomain,
            RootDomain: "autotf.de",
            Email: pleskEmail,
            AuthentikHost: authentikHost
        }
    };

    fetch('/api/manage/create', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    })
        .then(response => {
            if (!response.ok) throw new Error('Request failed');
            return response.json();
        })
        .then(result => {
            console.log('Container created successfully:', result);
        })
        .catch(error => {
            console.error('Error creating container:', error);
        })
        .then(x => {
            fetchManaged();
            document.getElementById('createDialog').classList.remove('open')
            invokeLoadingScreen(false);
        })
}

function toggleCollapse(id) {
    const el = document.getElementById(id);
    el.classList.toggle('show');
}