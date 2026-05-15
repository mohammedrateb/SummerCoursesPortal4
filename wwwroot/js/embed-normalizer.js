/* ════════════════════════════════════════════════════════════════
   Embed URL Normalizer — Shared (Create.cshtml + Edit.cshtml)

   Goals:
   • Convert "regular" share URLs into proper /embed URLs.
   • Accept pasted <iframe ...> HTML and extract the src.
   • Support: YouTube (watch / youtu.be / shorts / playlist), Vimeo,
     Google Drive (file/folder), Google Maps, Facebook, Instagram,
     TikTok, Twitter/X, Loom, SoundCloud, Spotify, Dailymotion.
   • Validate and surface unsupported URLs to the user.
   ════════════════════════════════════════════════════════════════ */
(function (root) {
    'use strict';

    function extractIframeSrc(input) {
        // If user pasted a full <iframe ...> HTML snippet, pull out the src=
        var m = input.match(/<iframe[^>]*\ssrc\s*=\s*["']([^"']+)["']/i);
        return m ? m[1] : input;
    }

    function detectAndNormalize(raw) {
        var input = (raw || '').trim();
        if (!input) return null;

        // 1) Allow pasting raw <iframe> HTML
        var url = extractIframeSrc(input).trim();

        // 2) YouTube playlist
        var m = url.match(/[?&]list=([\w-]{10,})/);
        if (m && /youtube\.com|youtu\.be/.test(url)) {
            return ok('YouTube Playlist', 'fa-youtube',
                'https://www.youtube.com/embed/videoseries?list=' + m[1]);
        }

        // 3) YouTube video (watch / youtu.be / shorts / embed / live)
        m = url.match(/(?:youtube\.com\/(?:watch\?(?:.*&)?v=|shorts\/|embed\/|live\/)|youtu\.be\/)([\w-]{6,})/);
        if (m) {
            var t = (url.match(/[?&]t=(\d+)/) || [])[1];
            var src = 'https://www.youtube.com/embed/' + m[1];
            if (t) src += '?start=' + t;
            return ok('YouTube', 'fa-youtube', src);
        }

        // 4) Vimeo
        m = url.match(/vimeo\.com\/(?:video\/)?(\d+)/);
        if (m) return ok('Vimeo', 'fa-vimeo-v', 'https://player.vimeo.com/video/' + m[1]);

        // 5) Dailymotion
        m = url.match(/dailymotion\.com\/(?:video\/|embed\/video\/)?([a-z0-9]+)/i);
        if (m) return ok('Dailymotion', 'fa-play', 'https://www.dailymotion.com/embed/video/' + m[1]);

        // 6) Google Drive — file
        m = url.match(/drive\.google\.com\/(?:file\/d\/|open\?id=|uc\?(?:export=view&)?id=)([\w-]{10,})/);
        if (m) return ok('Google Drive — ملف', 'fa-google-drive',
            'https://drive.google.com/file/d/' + m[1] + '/preview');

        // 7) Google Drive — folder
        m = url.match(/drive\.google\.com\/drive\/folders\/([\w-]{10,})/);
        if (m) return ok('Google Drive — مجلد', 'fa-google-drive',
            'https://drive.google.com/embeddedfolderview?id=' + m[1] + '#list');

        // 8) Google Docs / Sheets / Slides / Forms
        m = url.match(/docs\.google\.com\/(document|spreadsheets|presentation|forms)\/d\/([\w-]+)/);
        if (m) {
            var kind = m[1];
            var id = m[2];
            var path = (kind === 'forms') ? '/viewform?embedded=true' :
                       (kind === 'presentation') ? '/embed' : '/preview';
            return ok('Google ' + kind, 'fa-file-lines',
                'https://docs.google.com/' + kind + '/d/' + id + path);
        }

        // 9) Google Maps — accept embed URL or convert maps?q=
        if (/google\.[\w.]+\/maps\/embed/.test(url))
            return ok('Google Maps', 'fa-map-location-dot', url);
        m = url.match(/google\.[\w.]+\/maps(?:\/place)?\/(?:[^/?]+\/)?(?:@([-\d.]+),([-\d.]+))/);
        if (m) return ok('Google Maps', 'fa-map-location-dot',
            'https://maps.google.com/maps?q=' + m[1] + ',' + m[2] + '&z=15&output=embed');

        // 10) Facebook (videos / posts)
        if (/facebook\.com\//.test(url) && !/facebook\.com\/plugins\//.test(url)) {
            var kind = /\/videos?\//.test(url) ? 'video' : 'post';
            var src = 'https://www.facebook.com/plugins/' + kind + '.php?href=' +
                      encodeURIComponent(url) + '&show_text=true';
            return ok('Facebook', 'fa-facebook', src);
        }
        if (/facebook\.com\/plugins\//.test(url))
            return ok('Facebook', 'fa-facebook', url);

        // 11) Instagram (post / reel / tv)
        m = url.match(/instagram\.com\/(p|reel|tv)\/([\w-]+)/);
        if (m) return ok('Instagram', 'fa-instagram',
            'https://www.instagram.com/' + m[1] + '/' + m[2] + '/embed');

        // 12) TikTok
        m = url.match(/tiktok\.com\/@[\w.-]+\/video\/(\d+)/);
        if (m) return ok('TikTok', 'fa-tiktok', 'https://www.tiktok.com/embed/v2/' + m[1]);

        // 13) Twitter / X — use Twitframe (Twitter's own embed needs JS, Twitframe is a clean iframe)
        m = url.match(/(?:twitter\.com|x\.com)\/[\w]+\/status\/(\d+)/);
        if (m) return ok('Twitter / X', 'fa-x-twitter',
            'https://twitframe.com/show?url=' + encodeURIComponent(url));

        // 14) Loom
        m = url.match(/loom\.com\/(?:share|embed)\/([a-f0-9]{20,})/);
        if (m) return ok('Loom', 'fa-video', 'https://www.loom.com/embed/' + m[1]);

        // 15) SoundCloud
        if (/soundcloud\.com\//.test(url))
            return ok('SoundCloud', 'fa-soundcloud',
                'https://w.soundcloud.com/player/?url=' + encodeURIComponent(url) +
                '&color=%23ff5f03&auto_play=false&hide_related=false&show_comments=true&show_user=true');

        // 16) Spotify
        m = url.match(/open\.spotify\.com\/(track|album|playlist|episode|show|artist)\/([a-zA-Z0-9]+)/);
        if (m) return ok('Spotify', 'fa-spotify',
            'https://open.spotify.com/embed/' + m[1] + '/' + m[2]);

        // 17) Generic HTTPS — accept as-is so power users can paste any embed URL
        try {
            var u = new URL(url);
            if (u.protocol === 'https:' || u.protocol === 'http:')
                return ok('رابط مخصص', 'fa-link', url);
        } catch (_) { /* invalid URL */ }

        return {
            type: 'رابط غير صالح',
            icon: 'fa-circle-exclamation',
            normalized: url,
            valid: false
        };

        function ok(type, icon, normalized) {
            return { type: type, icon: icon, normalized: normalized, valid: true };
        }
    }

    root.EmbedNormalizer = { detectAndNormalize: detectAndNormalize };
})(window);
