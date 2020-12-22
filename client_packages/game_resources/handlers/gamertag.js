var _slicedToArray = function () { function sliceIterator(arr, i) { var _arr = []; var _n = true; var _d = false; var _e = undefined; try { for (var _i = arr[Symbol.iterator](), _s; !(_n = (_s = _i.next()).done); _n = true) { _arr.push(_s.value); if (i && _arr.length === i) break; } } catch (err) { _d = true; _e = err; } finally { try { if (!_n && _i["return"]) _i["return"](); } finally { if (_d) throw _e; } } return _arr; } return function (arr, i) { if (Array.isArray(arr)) { return arr; } else if (Symbol.iterator in Object(arr)) { return sliceIterator(arr, i); } else { throw new TypeError("Invalid attempt to destructure non-iterable instance"); } }; }();

mp.nametags.enabled = false;

var showGamertags = true;
var reupdateTagLabel = [];
var tagLabelPool = [];

var playerPos = void 0;
var playerTarget = void 0;
var playerAimAt = void 0;
var width = 0.025;
var height = 0.004;
var border = 0.001;

mp.keys.bind(global.Keys.VK_5, false, function () {
    if (!global.loggedin || global.chatActive || global.editing || global.menuCheck()) return;
    showGamertags = !showGamertags;
});
function calculateDistance(v1, v2) {
    var dx = v1.x - v2.x;
    var dy = v1.y - v2.y;
    var dz = v1.z - v2.z;

    return Math.sqrt(dx * dx + dy * dy + dz * dz);
}
mp.events.add('newFriend', function (player, pass) {
    if (player && mp.players.exists(player)) {
        mp.storage.data.friends[player.name] = true;
        mp.storage.flush();
    }
});
mp.events.add('render', function (nametags) {

    if (!global.loggedin) return;
    playerPos = mp.players.local.position;
    playerAimAt = mp.game.player.getEntityIsFreeAimingAt();
    playerTarget = mp.players.local;
    var isAdmin = global.localplayer.getVariable('IS_ADMIN');
    if (isAdmin == true) {
        var player = playerTarget;
        if (player === undefined || player.handle === undefined || !player.handle) player = playerAimAt;
        if (player === undefined || player.handle === undefined || !player.handle) {} else {
            if (player.getType() === 4 && player != global.localplayer) {
                mp.game.graphics.drawText(player.name + ' (' + player.remoteId + ')', [0.46, 0.4], { font: 4, color: [255, 255, 255, 235], scale: [2, 0.35], outline: true });
            }
        }
    }
    if (showGamertags) {

        preDraw();

        nametags.forEach(nametag => {
            let [player, x, y, distance] = nametag;
            
            if(player.getVariable('INVISIBLE') != true && player.getVariable('HideNick') != true)
            {
                var passportText = '';
                if (global.passports[player.name] !== undefined) passportText = ' | ' + global.passports[player.name];

                var text = '';
                var tag = player.getVariable('REMOTE_ID');
                var localFraction = global.localplayer.getVariable('fraction');
                var playerFraction = player.getVariable('fraction');
				
				text = player.getVariable('IS_MASK') ? ( global.localplayer.getVariable('IS_ADMIN') || localFraction != null && playerFraction != null && localFraction === playerFraction ? player.name + ' [' + tag + ']' + '\n в маске' : 'Гражданин [' + tag + '] \n в маске') : (global.localplayer.getVariable('IS_ADMIN') || localFraction != null && playerFraction != null && localFraction === playerFraction || mp.storage.data.friends[player.name] ? player.name + ' [' + tag + ']' : 'Гражданин [' + tag + ']' );
				text = text + ( localFraction != null && playerFraction != null && localFraction === playerFraction ? '\n' +  player.getVariable('fractionRankName') + '\n~c~#' + player.getVariable('PERSON_SID') : '\n~c~#' + player.getVariable('PERSON_SID'));

                if (player.getVariable('IS_ADMIN') && player.getVariable('REDNAME') == true)
                {
                    text = text + '\n ~r~Администратор проекта'
                }

                var color = (player.getVariable('REDNAME') == true) ? [255, 0, 0, 255] : [255, 255, 255, 255];
    
                if (player.vehicle) y += 0.065;
                drawPlayerTag(player, x, y, text, color);
                drawPlayerVoiceIcon(player, x, y);
				
                if (player.getVariable('InDeath'))
                {
                    drawPlayerTag(player, x, y - 0.03, "Гражданин в коме", [0,86,214,255]);
                }	
            }           
        })
    }
});

function distanceVector(v1, v2)
{
    var dx = (v1.x - v2.x), dy = (v1.y - v2.y), dz = (v1.z - v2.z);
    return Math.sqrt( dx * dx + dy * dy + dz * dz );
}

function preDraw()
{
    gameplayCamPos = mp.players.local.position;
}

function drawPlayerTag(player, x, y, displayname, color) {
    mp.game.graphics.drawText(displayname, [x, y], { font: 4, color: color, scale: [0.35, 0.35], outline: true });
    if (playerTarget != undefined && player.handle == playerTarget.handle || playerAimAt != undefined && player.handle == playerAimAt.handle || global.spectating) {
        y += 0.04;
        var health = player.getHealth();
        health = health <= 100 ? health / 100 : (health - 100) / 100;

        var armour = player.getArmour() / 100;
        if (armour > 0) {
            mp.game.graphics.drawRect(x, y, width + border * 2, height + border * 2, 0, 0, 0, 200);
            mp.game.graphics.drawRect(x, y, width, height, 150, 150, 150, 255);
            mp.game.graphics.drawRect(x - width / 2 * (1 - health), y, width * health, height, 255, 255, 255, 200);
            y -= 0.007;
            mp.game.graphics.drawRect(x, y, width + border * 2, height + border * 2, 0, 0, 0, 200);
            mp.game.graphics.drawRect(x, y, width, height, 41, 66, 78, 255);
            mp.game.graphics.drawRect(x - width / 2 * (1 - armour), y, width * armour, height, 48, 108, 135, 200);
        } else {
            mp.game.graphics.drawRect(x, y, width + border * 2, height + border * 2, 0, 0, 0, 200);
            mp.game.graphics.drawRect(x, y, width, height, 150, 150, 150, 255);
            mp.game.graphics.drawRect(x - width / 2 * (1 - health), y, width * health, height, 255, 255, 255, 200);
        }
    }
}

function drawPlayerVoiceIcon(player, x, y) {
    if (player.isVoiceActive) drawVoiceSprite("mpleaderboard", 'leaderboard_audio_3', [0.7, 0.7], 0, [255, 255, 255, 255], x, y - 0.02 * 0.7);else if (player.getVariable('voice.muted') == true) drawVoiceSprite("mpleaderboard", 'leaderboard_audio_mute', [0.7, 0.7], 0, [255, 0, 0, 255], x, y - 0.02 * 0.7);
}

function drawVoiceSprite(dist, name, scale, heading, colour, x, y, layer) {
    var resolution = mp.game.graphics.getScreenActiveResolution(0, 0),
        textureResolution = mp.game.graphics.getTextureResolution(dist, name),
        textureScale = [scale[0] * textureResolution.x / resolution.x, scale[1] * textureResolution.y / resolution.y];

    if (mp.game.graphics.hasStreamedTextureDictLoaded(dist) === 1) {
        if (typeof layer === 'number') mp.game.graphics.set2dLayer(layer);
        mp.game.graphics.drawSprite(dist, name, x, y, textureScale[0], textureScale[1], heading, colour[0], colour[1], colour[2], colour[3]);
    } else mp.game.graphics.requestStreamedTextureDict(dist, true);
}
