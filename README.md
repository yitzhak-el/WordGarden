# ממלכת המילים | Word Garden

A **Unity 2022.3 LTS** portrait-first, Hebrew-guided English-learning prototype for young absolute beginners. Original art and a distinct game identity. Not a Brawl Stars clone.

## Play now in Unity

1. Open this folder as a Unity project in Unity Hub with Unity 2022.3 LTS and install the Android Build Support module only if you want an Android build.
2. Open `Assets/Scenes/WordGarden.unity`, press Play. The runtime bootstrap creates the responsive UI automatically. The first scene is deliberately empty; that is expected.
3. Choose **מסע לימוד** (solo learning) or **דו קרב על אותו מכשיר** (six-round two-player pass-the-device duel). Tap the answer tiles. The headphone button speaks on Android using the device's TextToSpeech provider; the Editor shows the English words as a fallback. For an Editor view, set Game view to 9:16, e.g. 1080 × 1920.
4. To prepare Android: `Word Garden > Prepare mobile build`, then `File > Build Settings > Android > Switch Platform` and build. The menu registers the scene and portrait orientation. Adjust application ID, signing, privacy disclosures and content before any release.

No Unity Editor or Android device is available in the creation environment, so this project is **source-complete but not compiled or device-tested**. If your editor shows an import error, send the actual Console output for a targeted fix.

## What works

- Eight hand-authored steps: picture-to-word, sound-to-word, simple sentence building with ordered tiles and return review; three concepts (cat, dog, sun), then sample sentences. Each wrong solo answer enters a short review queue; progress and streak are saved locally to PlayerPrefs. An incorrect answer does not block a child forever; the challenge is revisited after a step. There are no punitive streak rewards.
- Six-round **local turn-based** two-player duel with shared challenges and separate scores. No accounts, chats, public leaderboards, or collection of child personal data.
- Code-generated uGUI cards, portrait layout, large targets, high-contrast original animal/sun illustrations, Hebrew onboarding copy, and separate English learning text. Android device TTS is a provisional audio source, not vetted recorded narration.

## Scope and the road to a real product

This is a *vertical slice*, not a $30M production game. It has **no remote multiplayer**. For online turns, add a server-authoritative match store: match ID, opaque player IDs, challenge ID/version, current turn, turn deadline, answer event, scored outcome and version; server validates answers, advances atomically and sends push notifications. Client does not contain secrets or decide server scores. Do not release online play for children until parental consent, age-appropriate account design, moderation/abuse controls, privacy and safeguarding review are complete. The local two-player mode tests turn rhythm without creating a child identity risk.

The learning loop should develop by evidence, not just more trivia: identify concept from image + recorded English audio, recall from audio alone, map text, use in a meaningful sentence, then produce a sentence with movable word tiles, followed by spaced review after hours/days. Vary contexts and distractors so kids cannot learn answer position or icons instead of language. Add learner testing with Hebrew-speaking children of different ages, reading ability, hearing needs and English levels; a language educator should review the item bank, phonics sequence, comprehension and feedback. Today's sentence step is a four-tile sentence assembly, but does not yet test free composition, speech, or transfer to new contexts. Do not present this prototype as pedagogically validated.

Visual direction for later production: a friendly floating garden with tactile cards and expressive companions, nuanced motion and sound, strong silhouettes and accessible colors; keep game rules calm and non-combative. Replace the sample illustrations with a cohesive art pipeline (sprite atlases, animation, cutscenes, UI transitions), recorded English voice with a consistent accent, Hebrew voice guidance, legible RTL typography tested on actual Android/iOS hardware, accessibility, offline content packs, content versioning and localization. The present Hebrew UI uses a simple glyph reversal workaround for Unity's legacy text rendering; this does not handle mixed-direction text or shaping reliably. Production must replace it with a proper RTL-capable text system and test punctuation and numerals on device.

## Repo layout

- `Assets/Scripts/Runtime/LearningModel.cs`: content records, learner review, save, local duel model.
- `Assets/Scripts/Runtime/GameController.cs`: touch UI, interactions, feedback and session navigation.
- `Assets/Scripts/Runtime/Art.cs`: UI corner sprite and illustration loading.
- `Assets/Scripts/Runtime/Speech.cs`: provisional Android TextToSpeech; Editor text fallback.
- `Assets/Scripts/Editor/BuildSetup.cs`: mobile build preparation.
- `Assets/Resources/Art/`: original sample illustrated concepts.
- `Assets/Resources/Fonts/`: DejaVu Sans, licensed under the accompanying license.

The existing `yitzhak-el.github.io` web game is a separate project. This source is hosted at https://github.com/yitzhak-el/WordGarden on `main`.

## WebGL build status

The project has **not yet compiled or been tested in Unity**. No playable URL exists. The former GameCI GitHub Actions workflow was removed because Isaac's current Unity Personal entitlement XML is not compatible with its legacy activation path. Do not add the entitlement file or account credentials to this repository.

The next route is Unity Build Automation (UBA): connect this repository as a Git source, create a WebGL target for Unity 2022.3.62f3 using `main`, and set its scene list to `Scenes/WordGarden.unity` relative to `Assets`. The repo root already contains `Assets`, `Packages`, and `ProjectSettings`; no project subfolder is needed. This scene starts empty by design, and the runtime bootstrap creates the UI. After a successful UBA build, download the complete WebGL artifact and deploy it to a static host; a build artifact by itself is not a playable website. Verify the served page in a browser before sharing it. Unity's build setup documentation: https://docs.unity.com/build-automation/basic-build-configuration/overview and https://docs.unity.com/build-automation/advanced-build-configuration/specify-the-scene-to-be-built.md.

On WebGL, the current audio button shows a written word rather than providing spoken audio. Listening steps are **not valid listening assessment** until recorded, licensed clips or suitable browser speech support are added and tested.
