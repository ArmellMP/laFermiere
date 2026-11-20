// Message de réussite transmis d'un écran à l'autre quand on revient en arrière
// (le retour dans l'historique ne peut pas transporter de données).
/** Message laissé par la saisie du suivi quand elle revient en arrière (effacé dès qu'il est affiché). */
const CLE_CELEBRATION = 'lafermiere.celebration.'

export function laisserCelebration(bandeId: number, message: string) {
  try {
    sessionStorage.setItem(CLE_CELEBRATION + bandeId, message)
  } catch {
    /* stockage indisponible : pas de message, sans gravité */
  }
}

export function lireCelebration(id?: string): string | null {
  try {
    return sessionStorage.getItem(CLE_CELEBRATION + id)
  } catch {
    return null
  }
}

export function oublierCelebration(id?: string) {
  try {
    sessionStorage.removeItem(CLE_CELEBRATION + id)
  } catch {
    /* rien à faire */
  }
}
