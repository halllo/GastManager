angular.module('gastmanager.services', [])

.factory('SignalR', function ($rootScope) {
  
  var chat = null;

  try {
    //$.connection.hub.url = "http://mbus.de:8000/signalr";
    $.connection.hub.url = "http://localhost:8000/signalr";
    chat = $.connection.myHub;
    chat.client.addMessage = function (name, message) {
      $rootScope.$emit("newMessage", name, message);
    };

    $.connection.hub.start().done(function () {
      chat.server.send("gastmanager.app", "Hallo");
    });
  }
  catch(e) {
  }

  return {
    emit: function (name, message) {
      if (chat != null) {
        chat.server.send(name, message);
      }
    }
  }
})
