// Default target date
const DEFAULT_TARGET_DATE = '2028-10-01T00:00:00';

// --- Translations ---
const TRANSLATIONS = {
    en: {
        days: 'Days', hours: 'Hours', minutes: 'Minutes', seconds: 'Seconds',
        settings: 'Settings',
        language: 'Language',
        targetDateLabel: 'Target Date & Time',
        apply: 'Apply',
        uploadPhotos: 'Upload Photos (slideshow)',
        clearPhotos: 'Clear Photos',
        targetDatePrefix: 'Target Date:',
        countdownTo: 'Countdown to',
        alertSelectDate: 'Please select a date and time.',
        confirmPastDate: 'The selected date is in the past. The countdown will show all zeros. Continue?',
        alertStorageError: 'Could not save images: storage limit may be exceeded. Try using fewer or smaller images.',
        prevImage: 'Previous image',
        nextImage: 'Next image',
        locale: 'en-US'
    },
    fr: {
        days: 'Jours', hours: 'Heures', minutes: 'Minutes', seconds: 'Secondes',
        settings: 'Paramètres',
        language: 'Langue',
        targetDateLabel: 'Date et heure cible',
        apply: 'Appliquer',
        uploadPhotos: 'Télécharger des photos (diaporama)',
        clearPhotos: 'Effacer les photos',
        targetDatePrefix: 'Date cible :',
        countdownTo: 'Compte à rebours jusqu\'au',
        alertSelectDate: 'Veuillez sélectionner une date et une heure.',
        confirmPastDate: 'La date sélectionnée est dans le passé. Le compte à rebours affichera zéro. Continuer ?',
        alertStorageError: 'Impossible de sauvegarder les images : la limite de stockage est peut-être dépassée. Essayez avec moins d\'images ou des images plus petites.',
        prevImage: 'Image précédente',
        nextImage: 'Image suivante',
        locale: 'fr-FR'
    },
    es: {
        days: 'Días', hours: 'Horas', minutes: 'Minutos', seconds: 'Segundos',
        settings: 'Configuración',
        language: 'Idioma',
        targetDateLabel: 'Fecha y hora objetivo',
        apply: 'Aplicar',
        uploadPhotos: 'Subir fotos (presentación)',
        clearPhotos: 'Borrar fotos',
        targetDatePrefix: 'Fecha objetivo:',
        countdownTo: 'Cuenta regresiva hasta el',
        alertSelectDate: 'Por favor selecciona una fecha y hora.',
        confirmPastDate: 'La fecha seleccionada está en el pasado. La cuenta regresiva mostrará ceros. ¿Continuar?',
        alertStorageError: 'No se pudieron guardar las imágenes: puede que se haya superado el límite de almacenamiento. Intenta con menos imágenes o imágenes más pequeñas.',
        prevImage: 'Imagen anterior',
        nextImage: 'Imagen siguiente',
        locale: 'es-ES'
    },
    zh: {
        days: '天', hours: '小时', minutes: '分钟', seconds: '秒',
        settings: '设置',
        language: '语言',
        targetDateLabel: '目标日期和时间',
        apply: '应用',
        uploadPhotos: '上传照片（幻灯片）',
        clearPhotos: '清除照片',
        targetDatePrefix: '目标日期：',
        countdownTo: '倒计时至',
        alertSelectDate: '请选择日期和时间。',
        confirmPastDate: '所选日期已过去，倒计时将显示零。是否继续？',
        alertStorageError: '无法保存图片：可能超出了存储限制。请尝试使用更少或更小的图片。',
        prevImage: '上一张',
        nextImage: '下一张',
        locale: 'zh-CN'
    }
};

function loadLanguage() {
    return localStorage.getItem('language') || 'en';
}

function saveLanguage(lang) {
    localStorage.setItem('language', lang);
}

let currentLang = loadLanguage();

function t(key) {
    return (TRANSLATIONS[currentLang]?.[key]) || TRANSLATIONS['en'][key] || key;
}

function applyTranslations() {
    document.getElementById('html-root').setAttribute('lang', t('locale').split('-')[0]);
    document.getElementById('label-days').textContent = t('days');
    document.getElementById('label-hours').textContent = t('hours');
    document.getElementById('label-minutes').textContent = t('minutes');
    document.getElementById('label-seconds').textContent = t('seconds');
    document.getElementById('settings-title').textContent = t('settings');
    document.getElementById('language-label').textContent = t('language');
    document.getElementById('target-date-label').textContent = t('targetDateLabel');
    document.getElementById('apply-date-btn').textContent = t('apply');
    document.getElementById('upload-photos-label').textContent = t('uploadPhotos');
    document.getElementById('clear-photos-btn').textContent = t('clearPhotos');
    document.getElementById('settings-toggle').setAttribute('aria-label', t('settings'));
    document.getElementById('prev-btn').setAttribute('aria-label', t('prevImage'));
    document.getElementById('next-btn').setAttribute('aria-label', t('nextImage'));
    updateDateDisplay();
}

// --- Target date ---
function loadTargetDate() {
    return localStorage.getItem('targetDate') || DEFAULT_TARGET_DATE;
}

function saveTargetDate(dateStr) {
    localStorage.setItem('targetDate', dateStr);
}

function formatDateDisplay(dateStr) {
    const d = new Date(dateStr);
    return d.toLocaleString(t('locale'), {
        year: 'numeric', month: 'long', day: 'numeric',
        hour: '2-digit', minute: '2-digit', second: '2-digit'
    });
}

let targetDateStr = loadTargetDate();
let targetDate = new Date(targetDateStr).getTime();

function updateDateDisplay() {
    const display = document.getElementById('target-date-display');
    const title = document.getElementById('countdown-title');
    const formatted = formatDateDisplay(targetDateStr);
    display.textContent = t('targetDatePrefix') + ' ' + formatted;
    title.textContent = t('countdownTo') + ' ' + new Date(targetDateStr).toLocaleDateString(t('locale'), { year: 'numeric', month: 'long', day: 'numeric' });
}

// --- Countdown ---
function updateCountdown() {
    const now = new Date().getTime();
    const distance = targetDate - now;

    if (distance < 0) {
        document.getElementById('days').textContent = '0';
        document.getElementById('hours').textContent = '0';
        document.getElementById('minutes').textContent = '0';
        document.getElementById('seconds').textContent = '0';
        return;
    }

    const days = Math.floor(distance / (1000 * 60 * 60 * 24));
    const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
    const seconds = Math.floor((distance % (1000 * 60)) / 1000);

    document.getElementById('days').textContent = days;
    document.getElementById('hours').textContent = hours.toString().padStart(2, '0');
    document.getElementById('minutes').textContent = minutes.toString().padStart(2, '0');
    document.getElementById('seconds').textContent = seconds.toString().padStart(2, '0');
}

// --- Slideshow ---
const DEFAULT_IMAGES = [
    'https://images.unsplash.com/photo-1506744038136-46273834b3fb?auto=format&fit=crop&w=1920&q=80',
    'https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=1920&q=80',
    'https://images.unsplash.com/photo-1465101099481-8da6ee8aa37b?auto=format&fit=crop&w=1920&q=80',
    'https://images.unsplash.com/photo-1519046904884-53103b34b206?auto=format&fit=crop&w=1920&q=80',
    'https://images.unsplash.com/photo-1499793983690-e29da59ef1c2?auto=format&fit=crop&w=1920&q=80',
    'https://images.unsplash.com/photo-1471922694854-ff1b63b20054?auto=format&fit=crop&w=1920&q=80',
    'https://images.unsplash.com/photo-1468413253725-0d5181091126?auto=format&fit=crop&w=1920&q=80',
    'https://images.unsplash.com/photo-1559494007-9f5847c49d94?auto=format&fit=crop&w=1920&q=80',
    'https://images.unsplash.com/photo-1548574505-5e239809ee19?auto=format&fit=crop&w=1920&q=80',
    'https://images.unsplash.com/photo-1566438480900-0609be27a4be?auto=format&fit=crop&w=1920&q=80'
];

function shuffleArray(arr) {
    const shuffled = arr.slice();
    for (let i = shuffled.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        const tmp = shuffled[i];
        shuffled[i] = shuffled[j];
        shuffled[j] = tmp;
    }
    return shuffled;
}

let defaultImages = shuffleArray(DEFAULT_IMAGES);
let slideshowImages = [];
let currentSlide = 0;
let slideshowInterval = null;

function loadSlideshowImages() {
    try {
        const stored = localStorage.getItem('slideshowImages');
        return stored ? JSON.parse(stored) : [];
    } catch (e) {
        return [];
    }
}

function saveSlideshowImages(images) {
    try {
        localStorage.setItem('slideshowImages', JSON.stringify(images));
    } catch (e) {
        alert(t('alertStorageError'));
    }
}

function updateBodyBackground(url) {
    document.body.style.backgroundImage = 'url("' + url.replace(/\\/g, '\\\\').replace(/"/g, '\\"') + '")';
}

function updateSlideshowUI() {
    const img = document.getElementById('slideshow-image');
    const controls = document.getElementById('slideshow-controls');
    const indicator = document.getElementById('slide-indicator');

    const activeImages = slideshowImages.length > 0 ? slideshowImages : defaultImages;

    img.src = activeImages[currentSlide];
    updateBodyBackground(activeImages[currentSlide]);
    controls.style.display = activeImages.length > 1 ? 'flex' : 'none';
    indicator.textContent = (currentSlide + 1) + ' / ' + activeImages.length;

    if (activeImages.length > 1 && !slideshowInterval) {
        slideshowInterval = setInterval(function() {
            currentSlide = (currentSlide + 1) % activeImages.length;
            updateSlideshowUI();
        }, 5000);
    }
}

function readFilesAsDataURLs(files, callback) {
    const results = [];
    let remaining = files.length;
    if (remaining === 0) { callback([]); return; }
    Array.from(files).forEach(function(file, i) {
        const reader = new FileReader();
        reader.onload = function(e) {
            results[i] = e.target.result;
            remaining--;
            if (remaining === 0) callback(results);
        };
        reader.readAsDataURL(file);
    });
}

// --- Settings panel ---
document.getElementById('settings-toggle').addEventListener('click', function() {
    const panel = document.getElementById('settings-panel');
    const expanded = !panel.hidden;
    panel.hidden = expanded;
    this.setAttribute('aria-expanded', String(!expanded));
});

document.getElementById('apply-date-btn').addEventListener('click', function() {
    const input = document.getElementById('target-date-input').value;
    if (!input) { alert(t('alertSelectDate')); return; }
    if (new Date(input).getTime() <= Date.now()) {
        if (!confirm(t('confirmPastDate'))) return;
    }
    targetDateStr = input;
    targetDate = new Date(targetDateStr).getTime();
    saveTargetDate(targetDateStr);
    updateDateDisplay();
    updateCountdown();
});

document.getElementById('photo-upload').addEventListener('change', function(e) {
    const files = e.target.files;
    if (!files || files.length === 0) return;
    readFilesAsDataURLs(files, function(dataURLs) {
        slideshowImages = slideshowImages.concat(dataURLs);
        currentSlide = 0;
        clearInterval(slideshowInterval);
        slideshowInterval = null;
        saveSlideshowImages(slideshowImages);
        updateSlideshowUI();
    });
    e.target.value = '';
});

document.getElementById('clear-photos-btn').addEventListener('click', function() {
    slideshowImages = [];
    currentSlide = 0;
    clearInterval(slideshowInterval);
    slideshowInterval = null;
    localStorage.removeItem('slideshowImages');
    updateSlideshowUI();
});

function navigateToSlide(newIndex) {
    const activeImages = slideshowImages.length > 0 ? slideshowImages : defaultImages;
    currentSlide = (newIndex + activeImages.length) % activeImages.length;
    clearInterval(slideshowInterval);
    slideshowInterval = null;
    updateSlideshowUI();
}

document.getElementById('prev-btn').addEventListener('click', function() {
    navigateToSlide(currentSlide - 1);
});

document.getElementById('next-btn').addEventListener('click', function() {
    navigateToSlide(currentSlide + 1);
});

document.getElementById('language-select').addEventListener('change', function() {
    currentLang = this.value;
    saveLanguage(currentLang);
    applyTranslations();
});

// --- Init ---
(function init() {
    // Set language selector to stored value
    document.getElementById('language-select').value = currentLang;

    // Pre-fill date input with stored value
    const inputEl = document.getElementById('target-date-input');
    // datetime-local value format: YYYY-MM-DDTHH:MM
    inputEl.value = targetDateStr.slice(0, 16);

    applyTranslations();
    updateCountdown();
    setInterval(updateCountdown, 1000);

    slideshowImages = loadSlideshowImages();
    updateSlideshowUI();
})();

