var HUD = new Vue({
  el: ".inGameHud",
  data: {
    show: false,
    ammo: 0,
    money: "117 000 000",
    bank: 0,
    mic: false,
    time: "00:00",
    date: "00.00.00",
    street: "iTeffa.com",
    crossingRoad: "Groups",
    server: 0,
    playerId: 0,
    online: 0,
    inVeh: false,
    belt: false,
    engine: false,
    doors: false,
    speed: 0,
    fuel: 0,
    hp: 0,
  },
  methods: {
    setTime: (time, date) => {
      this.time = time;
      this.date = date;
    },
  }
})

const chatmsgs = document.getElementById('chat_messages');
const chatvar = $("#chat");
const writeDo = 'do'
const writeMe = 'me'
const writeTry = 'try'
const writeB = 'b'

let chat = {
  size: 0,
  container: null,
  input: null,
  enabled: false,
  active: true,
  timer: null,
  alpha: 1,
  chatsize: 0,
  fontstep: 0,
  prevmsg: ["", "", "", "", "", "", "", "", "", ""],
  steplist: 0,
  backlist: 0,
  timestamp: 0,
  stime: "00:00"
};

function doInChat(slash) {
  switch (slash) {
  case "do":
    document.getElementsByTagName("input")[0].value = "/do "
    break;
  case "me":
    document.getElementsByTagName("input")[0].value = "/me "
    break;
  case "b":
    document.getElementsByTagName("input")[0].value = "/b "
    break;
  case "try":
    document.getElementsByTagName("input")[0].value = "/try "
    break;

  default:
    break;
  }
}

function enableChatInput(enable) {
  if (chat.active == false && enable == true) return;
  if (enable != (chat.input != null)) {
    mp.invoke("focus", enable);
    if (enable) {
      chatvar.css("opacity", 1);
      chat.input = chatvar.append('<div><input value="" id="chat_msg" type="text" /><div class="chat-btns"><button onclick="doInChat(writeDo)" style="width: 50px;height: 30px;border: 1px rgb(185 185 185) solid;background: rgb(66 66 66);color: #fff;margin-right: 3px;font-size: 17px;border-radius: 5px;margin-top: 5px;">/do</button><button onclick="doInChat(writeMe)" style="width: 50px;height: 30px;border: 1px rgb(185 185 185) solid;background: rgb(66 66 66);color: #fff;margin-right: 3px;font-size: 17px;border-radius: 5px;margin-top: 5px;">/me</button><button onclick="doInChat(writeTry)" style="width: 50px;height: 30px;border: 1px rgb(185 185 185) solid;background: rgb(66 66 66);color: #fff;margin-right: 3px;font-size: 17px;border-radius: 5px;margin-top: 5px;">/try</button><button onclick="doInChat(writeB)" style="width: 50px;height: 30px;border: 1px rgb(185 185 185) solid;background: rgb(66 66 66);color: #fff;margin-right: 3px;font-size: 17px;border-radius: 5px;margin-top: 5px;">/b</button></div></div></ul>').children(":last");
      chat.input.children("input").focus();
      mp.trigger("changeChatState", true);
    } else {
      chat.input.fadeOut('fast', function () {
        chat.input.remove();
        chat.input = null;
        mp.trigger("changeChatState", false);
      });
    }
  }
}

var chatAPI = {
  push: (text) => {
    chat.size++;

    if (chat.size >= 50) chat.container.children(":first").remove();
    if (chat.timestamp == 0) chat.container.append("<li>" + text + "</li>");
    else chat.container.append("<li>[" + chat.stime + "] " + text + "</li>");
    chat.container.scrollTop(9999);
  },

  clear: () => {
    chat.container.html("");
  },

  activate: (toggle) => {
    if (toggle == false && chat.input != null) enableChatInput(false);
    chat.active = toggle;
  },

  show: (toggle) => {
    if (toggle) chatvar.show();
    else chatvar.hide();
    chat.active = toggle;
  }
};

if (mp.events) {
  let api = {
    "chat:push": chatAPI.push,
    "chat:clear": chatAPI.clear,
    "chat:activate": chatAPI.activate,
    "chat:show": chatAPI.show
  };

  for (let fn in api) {
    mp.events.add(fn, api[fn]);
  }
}

function hide() {
  if (chat.alpha == 1) {
    if (chat.timer != null) clearTimeout(chat.timer);
    chat.timer = setTimeout(function () {
      chatvar.css("opacity", 0.65);
    }, 30000);
  }
}

function show() {
  if (chat.timer != null) {
    clearTimeout(chat.timer);
    chat.timer = null;
  }
  chatvar.css("opacity", 1);
}

function newcfg(a, b) {
  if (a == 0) {
    chat.timestamp = b;
    if (b == 0) chatAPI.push("");
    else chatAPI.push("");
  } else if (a == 1) {
    if (b == 3) b = 0;
    chat.chatsize = b;
    if (b == 0) chatmsgs.style.height = '160px';
    else if (b == 1) chatmsgs.style.height = '280px';
    else chatmsgs.style.height = '400px';
  } else if (a == 2) {
    if (b == 3) b = 0;
    chat.fontstep = b;
    if (b == 0) chatmsgs.style.fontSize = '1.4vh';
    else if (b == 1) chatmsgs.style.fontSize = '1.65vh';
    else chatmsgs.style.fontSize = '1.8vh';
  } else {
    chat.alpha = b;
    if (b == 0) {
      show();
      chatAPI.push("");
    } else chatAPI.push("");
  }
}

function savehistory(value) {
  if (chat.steplist < 9) {
    chat.prevmsg[chat.steplist] = value;
    chat.steplist++;
  } else {
    for (let i = 0; i != 9; i++) {
      chat.prevmsg[i] = chat.prevmsg[(i + 1)];
    }
    chat.prevmsg[chat.steplist] = value;
  }
}

var lastMessage = 0;

$(document).ready(function () {
  chat.container = $("#chat ul#chat_messages");
  hide();

  $(".ui_element").show();
  chatAPI.push("Добро пожаловать на сервер");

  $("body").keydown(function (event) {
    if (event.which == 84 && chat.input == null && chat.active == true) {
      enableChatInput(true);
      event.preventDefault();
      show();
      chat.backlist = chat.steplist;
    } else if (event.which == 13 && chat.input != null) {
      var value = chat.input.children("input").val();
      if (value.length > 0 && new Date().getTime() - lastMessage > 1000) {
        savehistory(value);
        lastMessage = new Date().getTime();
        if (value[0] == "/") {
          value = value.substr(1);
          var premade = value.split(' ')[0];
          if (value.length > 0 && value.length <= 150) {
            if (premade.includes("timestamp")) {
              newcfg(0, !chat.timestamp);
              mp.trigger('chatconfig', 0, chat.timestamp);
            } else if (premade.includes("pagesize")) {
              chat.chatsize++;
              newcfg(1, chat.chatsize);
              mp.trigger('chatconfig', 1, chat.chatsize);
            } else if (premade.includes("fontsize")) {
              chat.fontstep++;
              newcfg(2, chat.fontstep);
              mp.trigger('chatconfig', 2, chat.fontstep);
            } else if (premade.includes("chatalpha")) {
              newcfg(3, !chat.alpha);
              mp.trigger('chatconfig', 3, chat.alpha);
            } else mp.invoke("command", value);
          }
        } else {
          if (value.length <= 150) mp.invoke("chatMessage", value);
        }
      }
      chat.container.scrollTop(9999);
      enableChatInput(false);
      hide();
    } else if (event.which == 27 && chat.input != null) {
      enableChatInput(false);
      hide();
    } else if (event.which == 38 && chat.input != null) { // Листание вверх
      if (chat.steplist < 9) {
        if (chat.backlist >= 1) chat.backlist--;
        chat.input.children("input").val(chat.prevmsg[chat.backlist]);
      } else {
        chat.input.children("input").val(chat.prevmsg[chat.backlist]);
        if (chat.backlist >= 1) chat.backlist--;
      }
    } else if (event.which == 40 && chat.input != null) { // Листание вниз
      if (chat.backlist < chat.steplist) chat.backlist++;
      chat.input.children("input").val(chat.prevmsg[chat.backlist]);
    }
  });
});

var fxel = $('#effect');
var fx = {
  set: (style) => {
    fxel.addClass(style);
  },
  reset: () => {
    fxel.removeClass();
  }
}