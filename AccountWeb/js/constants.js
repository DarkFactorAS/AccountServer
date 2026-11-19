export const APP_TITLE = "DarkFactor - Account administration";

// Mirrors the AccountServer rules (AccountProvider.VerifyPassword / VerifyNickname) so mistakes
// show before submitting. The server stays the authority.
export const PASSWORD_MIN = 8;
export const PASSWORD_MAX = 20;
export const PASSWORD_PATTERN = /^[a-zA-Z0-9%*()]+$/;
export const NICKNAME_PATTERN = /^[a-zA-Z0-9]+$/;
