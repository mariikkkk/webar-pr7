mergeInto(LibraryManager.library, {
  PR7Publish: function(ptr) {
    var data = JSON.parse(UTF8ToString(ptr));
    window.pr7State = data;
    window.dispatchEvent(new CustomEvent('pr7state', {detail:data}));
  }
});
