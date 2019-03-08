const  apiUri = "http://localhost:5000/"
var loader;
var displayedDeviceId = 0;

$(() => {
    getDevices().then((res) => {
        loadDevice(res[0].serialNumber)
        loader = setInterval(() => loadDevice(res[0].serialNumber), 1000)
        res.map((o) => $("#deviceselector").append(new Option(o.name, o.serialNumber)))
        $('select').formSelect();
    })
    $("#deviceselector").on('change', (event) => {
        loadDevice(event.target.value);
        if(loader !== undefined){
            clearInterval(loader);
        }
        loader = setInterval(() => loadDevice(event.target.value), 1000);
    })
});

const getDevice = async (device) => {
    try {
        const response = await fetch(apiUri + 'api/devices/' + device, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        return await response.json();
    } catch (error) {
        console.log(error)
    }
    
}

const getDevices = async () => {
    try {
        const response = await fetch(apiUri + 'api/devices', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        return await response.json();
    } catch (error) {
        console.log(error)
    }
}

const startDevice = async () => {
    $('#startButton').addClass('disabled');
    try {
        var res = await fetch(apiUri + 'api/devices/' + displayedDeviceId + '/start', {
            method: 'PUT',
            body: "body",
            headers: {
                'Content-Type': 'application/json'
            }
        });
        if(res.status === 404){
            M.toast({html: 'Your device could not be found.'})
        }
        if(res.status === 405){
            M.toast({html: 'You are not allow to turn on the machine. Check credit!'})
        }
        if(res.status === 200){
            loadDevice(displayedDeviceId);
        }
    }catch(err){
        console.log(err)
    }
}

const stopDevice = async () => {
    $('#stopButton').addClass('disabled');
    var res = await fetch(apiUri+'api/devices/' + displayedDeviceId + '/stop', {
        method: 'PUT',
        body: "body",
        headers: {
            'Content-Type': 'application/json'
        }
    });
    if(res.status === 200){
        loadDevice(displayedDeviceId);
    }
}

const loadDevice = async (serialNumber) => {
    try {
        var data = await getDevice(serialNumber)
        displayedDeviceId = data.serialNumber;
        if(data.serialNumber != serialNumber ||  $('#deviceSerialNumber')[0].innerText === ""){
            $('#deviceSerialNumber').html(data.serialNumber);
        }
        $('#deviceName').html(data.name);
        $('#deviceType').html(data.type);
        $('#deviceWorkCounter').html(data.workCount);
        $('#deviceCredit').html(data.credit);
        if (data.isWorking === true) {
            $('#deviceIsWorking').html("Yes...");
            $('#startButton').addClass('disabled');
            $('#stopButton').removeClass('disabled');
            $("#machine").attr("src", "images/robotic-arm.gif");
        } else {
            $('#deviceIsWorking').html("No");
            $('#startButton').removeClass('disabled');
            $('#stopButton').addClass('disabled');
            $("#machine").attr("src", "images/robotic-arm.jpg");
        }      
        $('#deviceLastService').html(dateFormatter(data.lastService));
        $('#deviceNextService').html(dateFormatter(data.nextService));
    }catch (error){
        console.log(error);
    }
}

function dateFormatter(rawDate){
    function pad(s) { return (s < 10) ? '0' + s : s; }
    date =  new Date(rawDate);
    return [pad(date.getDate()), pad(date.getMonth() + 1), date.getFullYear()].join('.');
}

