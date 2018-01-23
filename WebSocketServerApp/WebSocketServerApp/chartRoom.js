///登入按钮点击事件
var ws;
var username;
function logInOnClickFunction(thisEle) {
    var input = thisEle.parentNode.children[3];
    var label = thisEle.parentNode.children[0]; 
    username = input.value;
    if (username != "") {
        label.innerText = input.value;
        thisEle.parentNode.classList.remove("notLoggedIn");
        thisEle.parentNode.classList.add("logIn");
        //initBaseMenu(input.value, thisEle.parentNode.parentNode);
        ws = new WebSocket("ws://192.168.2.84:60328/chat");
        ws.onopen = function (e) {
            var timestamp = Math.round(Date.now() / 1000);
            var protocol = { CommandType: "Login", UserName: username, Message: "", TimeStamp: timestamp };
            var protocolString = JSON.stringify(protocol);
            ws.send(protocolString);
        };
        ws.onclose = function (e) {
           

        };
        ws.onmessage = function (e) {
            var message = JSON.parse(e.data);
            console.log(e);
            if (message.CommandType == "Login") {
                var memberUl = document.getElementsByClassName("chatroom-list")[0];
                console.log(memberUl);
                var liNode = document.createElement("li");
                liNode.className = "selectGroup";
                liNode.innerText = message.UserName;
                console.log(liNode);
                memberUl.appendChild(liNode);
            }
            else if (message.CommandType == "Message") {
                var messageElement = document.getElementsByClassName("contentPart")[0];
                createNewInformation(messageElement, message);
            }
        };
        
    }
    else {
        document.getElementsByClassName("error-message")[0].innerText = "名字不能为空!";
    }
}
///登出按钮点击事件
function logOutOnClickFunction(thisEle) {
    thisEle.parentNode.children[3].value = "";
    thisEle.parentNode.children[0].innerText = "请先登录";
    thisEle.parentNode.classList.remove("logIn");
    thisEle.parentNode.classList.add("notLoggedIn");
    
}
///发送消息点击事件
function sendOnClickFunction() {

    var content = document.getElementsByClassName("message-panel")[0].value;  
    if (content == "") {
        document.getElementsByClassName("error-message").innerText = "发送内容不能为空!";
    }
    else {      
       
        var timestamp = Math.round(Date.now() / 1000);
        var protocol = { CommandType: "Message", UserName: username, Message: content, TimeStamp: timestamp };
        var protocolString = JSON.stringify(protocol);
        ws.send(protocolString);
    }
}

///初始化联系人列表
///name     ：登录名
///baseDiv  ：基础div
function initBaseMenu(name, baseDiv) {
    var list = [name];
    var ul = document.createElement("ul");
    for(var i = 0; i < list.length; i++){
        var li = document.createElement("li");
        li.onclick = function(){
            //移除之前内容
            var list = this.parentNode.getElementsByClassName("selectGroup");
            while(list.length){
                list[0].classList.remove("selectGroup");
            }
            this.className = "selectGroup"; //追加当前的
            //initDetailMenu(name, this.innerText, baseDiv);//初始化成员列表
            initChartPart(name, this.innerText, baseDiv);//初始化聊天区
        };

        li.innerText = list[i];
        ul.appendChild(li);
    }
    baseDiv.children[1].children[0].appendChild(ul);
    ul.children[0].click();
}
///初始化成员列表
///name     ：登录名
///groupName：联系人列表
///baseDiv  ：基础div
function initDetailMenu(name, groupName, baseDiv){
    //获得数据
    var detailList;
  
    //移除原来的ul
    var list = baseDiv.children[1].children[1].getElementsByTagName("ul");
    while(list.length){
        list[0].parentNode.removeChild(list[0]);
    }

    //创建内容并追加
    var ul = document.createElement("ul");
    for(var i = 0; i < detailList.length; i++){
        var li = document.createElement("li");
        li.innerHTML = "<li>" + detailList[i] + "</li>";
        ul.appendChild(li);
    }
    baseDiv.children[1].children[1].appendChild(ul);
}
///初始化聊天室
function initChartPart(name, groupName, baseDiv) {
    var createInformation = name + "创建于 " + new Date();

    //改标题
    var titleDiv = baseDiv.children[2].children[0];
    titleDiv.innerHTML = "<label class=\"titleLabel\">" + groupName + "</label><label class=\"detailLabel\">" + createInformation + "</label>";

    //删除原内容
    var content = baseDiv.children[2].getElementsByClassName("contentPart")[0].children;
    while(content.length)
        content[0].parentNode.removeChild(content[0]);

    //改具体内容
    //createNewInformation(baseDiv.children[2].children[1]);
    baseDiv.children[2].children[1].scrollTop = baseDiv.scrollHeight;
}

///添加消息
function createNewInformation(baseDiv, message) {
    var date = new Date(message.TimeStamp * 1000).toLocaleString();
    if (message.UserName == username) {
        baseDiv.innerHTML += "<span class=\"sayRow mySay\"><label class=\"informationLabel\">" + message.UserName + "     " + date + "</label><label class=\"contentLabel\">" + message.Message + "</label></span>";
    }
    else {
        baseDiv.innerHTML += "<span class=\"sayRow\"><label class=\"informationLabel\">" + message.UserName + "     " + date + "</label><label class=\"contentLabel\">" + message.Message + "</label></span>";
    }   
}

function CloseWebSocket() {
    if (ws != undefined) {
        ws.close();
    }
}