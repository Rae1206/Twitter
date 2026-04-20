-- =====================================================
-- Email Templates para la tabla dbo.EmailTemplates
--Ejecutar en: X
-- =====================================================

-- 1. Bienvenida (registro de nuevo usuario)
INSERT INTO dbo.EmailTemplates (Name, Subject, Body, CreatedAt)
VALUES 
(
    'Welcome',
    '¡Bienvenido a Twitter, {fullName}!',
    '<h1>¡Hola, {fullName}!</h1>
    <p>Nos alegra mucho que te hayas unido a Twitter.</p>
    <p>Ahora puedes:</p>
    <ul>
        <li>Crear y compartir tus pensamientos</li>
        <li>Seguir a otros usuarios</li>
        <li>Interactuar con publicaciones</li>
    </ul>
    <p>¡Empieza a explorar y conectar con personas!</p>
    <p>Saludos,<br>El equipo de Twitter</p>',
    sysutcdatetime()
);

-- 2. Recuperación de contraseña con OTP
INSERT INTO dbo.EmailTemplates (Name, Subject, Body, CreatedAt)
VALUES 
(
    'PasswordReset',
    'Recupera tu contraseña - Código de verificación',
    '<h1>Recuperación de contraseña</h1>
    <p>Hola {fullName},</p>
    <p>Has solicitado recuperar tu contraseña.</p>
    <p>Tu código de verificación es:</p>
    <h2 style="background: #f0f0f0; padding: 15px; text-align: center; letter-spacing: 5px;">{otp}</h2>
    <p>Este código expira en <strong>15 minutos</strong>.</p>
    <p>Si no solicitaste este cambio, puedes ignorar este email.</p>',
    sysutcdatetime()
);

-- 3. Notificación de cambio de contraseña
INSERT INTO dbo.EmailTemplates (Name, Subject, Body, CreatedAt)
VALUES 
(
    'PasswordChanged',
    'Tu contraseña ha sido cambiada',
    '<h1>Notificación de seguridad</h1>
    <p>Hola {fullName},</p>
    <p>Tu contraseña ha sido cambiada exitosamente.</p>
    <p>Si no fuiste tú quien realizó este cambio, por favor contacta con soporte inmediatamente.</p>
    <p>Saludos,<br>El equipo de Twitter</p>',
    sysutcdatetime()
);

-- 4. Confirmación de email (verificación de cuenta)
INSERT INTO dbo.EmailTemplates (Name, Subject, Body, CreatedAt)
VALUES 
(
    'VerifyEmail',
    'Confirma tu correo electrónico',
    '<h1>Bienvenido a Twitter</h1>
    <p>Hola {fullName},</p>
    <p>Para completar tu registro, por favor verifica tu correo electrónico.</p>
    <p>Tu código de verificación es:</p>
    <h2 style="background: #f0f0f0; padding: 15px; text-align: center; letter-spacing: 5px;">{otp}</h2>
    <p>Este código expira en <strong>24 horas</strong>.</p>',
    sysutcdatetime()
);