import { useEffect, useState, type ReactNode } from 'react'
import { Link, NavLink, useLocation } from 'react-router-dom'
import { api } from '../api/client'
import type { Alerte } from '../api/types'
import { useAuth } from '../auth/AuthContext'
import { entreeActive, navigationPour } from '../navigation'
import { useConfirmer } from './Confirmation'
import { IconeCloche, IconeFermer, IconeMenu, IconeSortie } from './Icones'

/**
 * Ossature de l'application connectée :
 *   - ordinateur : barre latérale fixe (arborescence complète) + barre d'en-tête
 *   - téléphone  : barre d'en-tête compacte + barre d'onglets en bas + menu coulissant
 */
export default function Coquille({ children }: { children: ReactNode }) {
  const { utilisateur, deconnexion } = useAuth()
  const confirmer = useConfirmer()
  const location = useLocation()
  const [menuOuvert, setMenuOuvert] = useState(false)
  const [nonLues, setNonLues] = useState(0)

  const groupes = navigationPour(utilisateur?.role)
  const principales = groupes.flatMap((g) => g.entrees).filter((e) => e.principale)
  const active = entreeActive(location.pathname)

  // Nombre d'alertes non lues, relu à chaque changement de page (et après un marquage)
  useEffect(() => {
    let annule = false
    api
      .get<Alerte[]>('/Alertes/non-lues')
      .then((a) => !annule && setNonLues(a.length))
      .catch(() => undefined)
    return () => {
      annule = true
    }
  }, [location.pathname, location.search])

  // Ferme le menu du téléphone après une navigation
  useEffect(() => setMenuOuvert(false), [location.pathname])

  if (!utilisateur) return null
  const initiales = utilisateur.nom
    .split(/\s+/)
    .map((m) => m[0])
    .join('')
    .slice(0, 2)
    .toUpperCase()

  const liens = (
    <nav className="nav" aria-label="Navigation principale">
      {groupes.map((g, i) => (
        <div key={i} className="nav-groupe">
          {g.titre && <p className="nav-titre">{g.titre}</p>}
          {g.entrees.map((e) => (
            <NavLink key={e.chemin} to={e.chemin} end={e.chemin === '/'} className="nav-lien">
              <span className="nav-icone">{e.icone}</span>
              <span>{e.libelle}</span>
              {e.chemin === '/alertes' && nonLues > 0 && <span className="nav-compteur">{nonLues}</span>}
            </NavLink>
          ))}
        </div>
      ))}
    </nav>
  )

  const profil = (
    <div className="profil">
      <span className="avatar">{initiales}</span>
      <div className="profil-texte">
        <strong>{utilisateur.nom}</strong>
        <span>{utilisateur.roleLibelle}</span>
      </div>
      <button className="bouton-icone discret" onClick={async () => {
          const ok = await confirmer({
            titre: 'Se déconnecter ?',
            message: 'Il faudra saisir à nouveau votre e-mail et votre mot de passe.',
            libelleConfirmer: 'Se déconnecter',
            ton: 'attention',
          })
          if (ok) void deconnexion()
        }} aria-label="Se déconnecter" title="Se déconnecter">
        <IconeSortie taille={20} />
      </button>
    </div>
  )

  return (
    <div className="coquille">
      <aside className="laterale">
        <Link to="/" className="laterale-marque">
          <img src="/icone.svg" alt="" width={36} height={36} />
          <span>
            La Fermière
            <small>Gestion d’élevage</small>
          </span>
        </Link>
        {liens}
        {profil}
      </aside>

      <div className="zone">
        <header className="entete-app">
          <button className="bouton-icone seulement-mobile" onClick={() => setMenuOuvert(true)} aria-label="Ouvrir le menu">
            <IconeMenu />
          </button>
          <Link to="/" className="entete-marque seulement-mobile">
            <img src="/icone.svg" alt="" width={28} height={28} />
            La Fermière
          </Link>
          <span className="entete-section seulement-ordinateur">{active?.libelle ?? 'La Fermière'}</span>
          <div className="entete-actions">
            <Link to="/alertes?etat=non-lues" className="bouton-icone cloche" aria-label={`Alertes : ${nonLues} non lue(s)`}>
              <IconeCloche />
              {nonLues > 0 && <span className="pastille-compteur">{nonLues > 99 ? '99+' : nonLues}</span>}
            </Link>
            <span className="avatar petit seulement-mobile" title={utilisateur.nom}>
              {initiales}
            </span>
          </div>
        </header>

        <div key={location.pathname} className="contenu transition-page">
          {children}
        </div>
      </div>

      <nav className="barre-onglets" aria-label="Navigation rapide">
        {principales.map((e) => (
          <NavLink key={e.chemin} to={e.chemin} end={e.chemin === '/'} className="onglet-bas">
            <span className="onglet-bas-icone">
              {e.icone}
              {e.chemin === '/alertes' && nonLues > 0 && <span className="pastille-compteur">{nonLues}</span>}
            </span>
            <span>{e.libelle === 'Tableau de bord' ? 'Accueil' : e.libelle}</span>
          </NavLink>
        ))}
        <button className="onglet-bas" onClick={() => setMenuOuvert(true)}>
          <span className="onglet-bas-icone">
            <IconeMenu />
          </span>
          <span>Plus</span>
        </button>
      </nav>

      <div className={`tiroir ${menuOuvert ? 'ouvert' : ''}`} aria-hidden={!menuOuvert}>
        <div className="tiroir-voile" onClick={() => setMenuOuvert(false)} />
        <div className="tiroir-panneau" role="dialog" aria-label="Menu">
          <div className="tiroir-haut">
            <span className="laterale-marque">
              <img src="/icone.svg" alt="" width={32} height={32} />
              <span>La Fermière</span>
            </span>
            <button className="bouton-icone discret" onClick={() => setMenuOuvert(false)} aria-label="Fermer le menu">
              <IconeFermer />
            </button>
          </div>
          {liens}
          {profil}
        </div>
      </div>
    </div>
  )
}
