// tiktok-connector.js
const { WebcastPushConnection } = require('tiktok-live-connector');
const readline = require('readline');
const connections = {};
const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
    terminal: false
});

rl.on('line', async (line) => {
    try {
        const request = JSON.parse(line);
        const { Command, Parameters } = request;
        let response;
        switch (Command) {
            case 'connect':
                response = await connect(Parameters.username, Parameters.options);
                break;
            case 'disconnect':
                response = disconnect(Parameters.username);
                break;
            case 'room-info':
                response = await getRoomInfo(Parameters.username);
                break;
            case 'available-gifts':
                response = await getAvailableGifts(Parameters.username);
                break;
            case 'send-message':
                response = await sendMessage(Parameters.username, Parameters.text, Parameters.sessionId);
                break;
            default:
                response = { status: 'error', message: 'Unknown command' };
        }
        process.stdout.write(JSON.stringify(response) + '\n');
    } catch (err) {
        process.stdout.write(JSON.stringify({ status: 'error', message: err.message }) + '\n');
    }
});

async function connect(username, options = {}) {
    if (connections[username]) {
        throw new Error('Connection already exists');
    }
    const tiktokLiveConnection = new WebcastPushConnection(username, options);
    tiktokLiveConnection.on('connected', state => {
        console.log(`Connected to roomId ${state.roomId}`);
    });
    tiktokLiveConnection.on('disconnected', () => {
        console.log('Disconnected :(');
        delete connections[username];
    });
    tiktokLiveConnection.on('streamEnd', (actionId) => {
        if (actionId === 3) {
            console.log('Stream ended by user');
        }
        if (actionId === 4) {
            console.log('Stream ended by platform moderator (ban)');
        }
    });
    tiktokLiveConnection.on('error', err => {
        console.error('Error!', err);
        delete connections[username];
    });
    tiktokLiveConnection.on('chat', data => {
        console.log(`${data.uniqueId} writes: ${data.comment}`);
    });
    tiktokLiveConnection.on('gift', data => {
        console.log(`${data.uniqueId} sends ${data.giftId}`);
    });
    await tiktokLiveConnection.connect();
    connections[username] = tiktokLiveConnection;
    return tiktokLiveConnection.getState();
}

function disconnect(username) {
    const connection = connections[username];
    if (!connection) {
        throw new Error('Connection does not exist');
    }
    connection.disconnect();
    delete connections[username];
    return { status: 'success' };
}

async function getRoomInfo(username) {
    const connection = connections[username];
    if (!connection) {
        throw new Error('Connection does not exist');
    }
    const roomInfo = await connection.getRoomInfo();
    return roomInfo;
}

async function getAvailableGifts(username) {
    const connection = connections[username];
    if (!connection) {
        throw new Error('Connection does not exist');
    }
    const gifts = await connection.getAvailableGifts();
    return gifts;
}

async function sendMessage(username, text, sessionId) {
    const connection = connections[username];
    if (!connection) {
        throw new Error('Connection does not exist');
    }
    await connection.sendMessage(text, sessionId);
    return { status: 'success' };
}