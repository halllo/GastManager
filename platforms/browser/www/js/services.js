angular.module('gastmanager.services', [])


.factory('SignalR', function (GastUpdates, $rootScope) {
  
  var chat = null;

  try {
    //$.connection.hub.url = "http://mbus.de:8000/signalr";
    $.connection.hub.url = "http://localhost:8000/signalr";
    chat = $.connection.myHub;
    chat.client.addMessage = function (name, message) {
      $rootScope.$emit("newMessage", name, message);
    };

    $.connection.hub.start().done(function () {
      chat.server.send("gastmanager.app", "hallo");
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


.factory('GastUpdates', function ($rootScope) {
  
  var gaeste = [];

  $rootScope.$on("newMessage", function (e, name, message) {
    if (name === "couchtisch" && message.startsWith("gäste;"))
    {
      var gs = message.split("\n");
      gaeste = [];
      for (var i = 1; i < gs.length; i++) {
        var gss = gs[i].split(";");
        gaeste.push({
          id: parseInt(gss[0]),
          name: gss[1],
          farbe: gss[2]
        });
      }
      $rootScope.$emit("gastUpdates");
    }
  });

  return {
    alle: function() { 
      return gaeste; 
    },
    fuerId: function(id) {
      for (var i = 0; i < gaeste.length; i++) {
        if (gaeste[i].id == id) {
          return gaeste[i];
        }
      }
      return {
        name: null,
        farbe: null
      };
    }
  }
})
