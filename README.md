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

The existing GitHub Pages web game is a separate project. This Unity code is meant for a **new, clearly named repo or an approved destination**; do not push it over that site's root. GitHub account connection and repo destination remain pending.

## WebGL CI and GitHub Pages

`.github/workflows/webgl.yml` builds this Unity project with GameCI on pushes to `main` and deploys its generated WebGL output to GitHub Pages. The workflow is included but **has not run**; no GitHub repo, build artifact or playable link exists yet. Unity's editor license activation is required on the CI runner. Follow the current GameCI activation instructions at https://game.ci/docs/github/activation/ and builder guidance at https://game.ci/docs/github/builder/ for the user's applicable Unity license type. Store only supported activation values (`UNITY_LICENSE` for a Personal license when applicable, or `UNITY_EMAIL`, `UNITY_PASSWORD`, `UNITY_SERIAL` for a licensed seat) as GitHub Actions encrypted secrets through a secure interface; do not put any credentials into this repository, chat or email. These secrets are not in this ZIP. Review Unity and GameCI license terms for cloud runner use. In the new repository's Settings > Pages, select **GitHub Actions** as the build/deploy source and allow Actions. Review failed Actions logs before sharing a link; the artifact path or activation may need adjustment for the installed GameCI version.

Do not assume Android TextToSpeech plays in WebGL. This vertical slice displays the target word on WebGL when the audio button is pressed, so sound-to-word activities are **not valid as listening assessment** there. Recorded, licensed clips or vetted browser speech support are required before claiming WebGL listening instruction works. If the user requires a fully functioning audio loop in the playable browser demo, complete that work before deployment.
