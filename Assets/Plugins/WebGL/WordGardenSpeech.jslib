mergeInto(LibraryManager.library, {
  WG_Speak: function(pointer) {
    if (typeof window === 'undefined' || !window.speechSynthesis || !window.SpeechSynthesisUtterance) return 0;
    try {
      var text = UTF8ToString(pointer);
      if (!text) return 0;
      window.speechSynthesis.cancel();
      var utterance = new window.SpeechSynthesisUtterance(text);
      utterance.lang = 'en-US';
      utterance.rate = 0.82;
      utterance.pitch = 1.06;
      var voices = window.speechSynthesis.getVoices();
      var english = voices.find(function(v){ return /^en[-_]US/i.test(v.lang); }) || voices.find(function(v){ return /^en/i.test(v.lang); });
      if (english) utterance.voice = english;
      // Retain the utterance while Chrome's speech engine plays it.
      window.wordGardenUtterance = utterance;
      utterance.onend = function(){ window.wordGardenUtterance = null; };
      window.speechSynthesis.speak(utterance);
      return 1;
    } catch (e) {
      console.warn('WordGarden audio unavailable:', e);
      return 0;
    }
  }
});
