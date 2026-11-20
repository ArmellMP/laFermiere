// Formulaires de création : bande, clôture, vente, achat, client, fournisseur.
// Chaque formulaire s'affiche dans un <Panneau>, reprend les règles de l'API
// (valeurs autorisées, bornes) et affiche ses erreurs champ par champ.
import { useState } from 'react'
import { api } from '../api/client'
import { oublierNoms } from '../api/noms'
import type { Achat, Bande, Client, Fournisseur, Vente } from '../api/types'
import { useDonnees } from '../components/Chargement'
import { useConfirmer } from '../components/Confirmation'
import { Champ, FormulairePanneau, Interrupteur, Ligne, Panneau, Segments, useEnvoi, versNombre } from '../components/Formulaire'
import { estActive } from '../metier'
import { aujourdhui, monnaie, nombre, partieDate } from '../outils'

interface PropsFormulaire<T> {
  ouvert: boolean
  onFermer: () => void
  onCree: (cree: T) => void
}

// ===========================================================================
// Nouvelle bande
// ===========================================================================
export function FormulaireBande({ ouvert, onFermer, onCree }: PropsFormulaire<Bande>) {
  return (
    <Panneau ouvert={ouvert} onFermer={onFermer} titre="Nouvelle bande" sousTitre="Un lot de poussins arrivé ensemble.">
      <CorpsBande onFermer={onFermer} onCree={onCree} />
    </Panneau>
  )
}

function CorpsBande({ onFermer, onCree }: Omit<PropsFormulaire<Bande>, 'ouvert'>) {
  const [date, setDate] = useState(aujourdhui())
  const [poussins, setPoussins] = useState('')
  const [observations, setObservations] = useState('')
  const { envoi, erreur, champs, envoyer, signaler } = useEnvoi()

  const valider = () => {
    const n = versNombre(poussins)
    if (!n || n < 1 || n > 50000 || !Number.isInteger(n)) return signaler('nbPoussinsInit', 'Indiquez un nombre de poussins entre 1 et 50 000.')
    void envoyer(async () => {
      const bande = await api.post<Bande>('/Bandes', { dateArrivee: date, nbPoussinsInit: n, observations: observations.trim() || null })
      oublierNoms()
      onCree(bande)
    })
  }

  return (
    <FormulairePanneau onSubmit={valider} erreur={erreur} envoi={envoi} libelleValider="Créer la bande" onAnnuler={onFermer}>
      <Champ libelle="Date d’arrivée" obligatoire erreurs={champs.dateArrivee} aide="Le code de la bande (AAAA-MM-JJ-001) est attribué automatiquement.">
        <input type="date" value={date} max={aujourdhui()} onChange={(e) => setDate(e.target.value)} />
      </Champ>
      <Champ libelle="Nombre de poussins reçus" obligatoire erreurs={champs.nbPoussinsInit}>
        <input type="text" inputMode="numeric" value={poussins} onChange={(e) => setPoussins(e.target.value)} placeholder="Ex. : 500" />
      </Champ>
      <Champ libelle="Observations" erreurs={champs.observations}>
        <textarea rows={3} maxLength={2000} value={observations} onChange={(e) => setObservations(e.target.value)} placeholder="Souche, couvoir, état à l’arrivée…" />
      </Champ>
    </FormulairePanneau>
  )
}

// ===========================================================================
// Démarrage de l'élevage (En préparation -> En cours)
// ===========================================================================
export function FormulaireDemarrage({ ouvert, onFermer, onCree, bande }: PropsFormulaire<Bande> & { bande: Bande }) {
  return (
    <Panneau ouvert={ouvert} onFermer={onFermer} titre={`Démarrer ${bande.code}`} sousTitre="Les poussins sont installés : l’élevage commence.">
      <CorpsDemarrage bande={bande} onFermer={onFermer} onCree={onCree} />
    </Panneau>
  )
}

function CorpsDemarrage({ bande, onFermer, onCree }: Omit<PropsFormulaire<Bande>, 'ouvert'> & { bande: Bande }) {
  const { envoi, erreur, envoyer } = useEnvoi()
  const confirmer = useConfirmer()
  const valider = async () => {
    const ok = await confirmer({
      titre: `Démarrer l’élevage de ${bande.code} ?`,
      message: 'La bande passera « En cours ». On ne peut pas la remettre en préparation ensuite.',
      libelleConfirmer: 'Oui, démarrer',
      ton: 'attention',
    })
    if (!ok) return
    void envoyer(async () => {
      const demarree = await api.put<Bande>(`/Bandes/${bande.id}`, {
        id: bande.id,
        nbPoussinsActuel: bande.nbPoussinsActuel,
        observations: bande.observations ?? null,
        statut: 'EnCours',
      })
      onCree(demarree)
    })
  }

  return (
    <FormulairePanneau onSubmit={() => void valider()} erreur={erreur} envoi={envoi} libelleValider="Démarrer l’élevage" onAnnuler={onFermer}>
      <div className="encart-info">
        <strong>Ce qui change</strong>
        <p>La bande passe au statut « En cours ». Elle pourra être clôturée en fin de cycle ; tant qu’elle est en préparation, elle ne peut pas l’être.</p>
      </div>
      <dl className="fiche">
        <div>
          <dt>Arrivée</dt>
          <dd>{partieDate(bande.dateArrivee).split('-').reverse().join('/')}</dd>
        </div>
        <div>
          <dt>Poussins</dt>
          <dd>{nombre(bande.nbPoussinsActuel)}</dd>
        </div>
      </dl>
    </FormulairePanneau>
  )
}

// ===========================================================================
// Clôture d'une bande
// ===========================================================================
export function FormulaireCloture({ ouvert, onFermer, onCree, bande }: PropsFormulaire<Bande> & { bande: Bande }) {
  return (
    <Panneau ouvert={ouvert} onFermer={onFermer} titre={`Clôturer ${bande.code}`} sousTitre="Fin du cycle : la bande ne recevra plus de suivi.">
      <CorpsCloture bande={bande} onFermer={onFermer} onCree={onCree} />
    </Panneau>
  )
}

function CorpsCloture({ bande, onFermer, onCree }: Omit<PropsFormulaire<Bande>, 'ouvert'> & { bande: Bande }) {
  const [date, setDate] = useState(aujourdhui())
  const [observations, setObservations] = useState('')
  const { envoi, erreur, champs, envoyer } = useEnvoi()
  const confirmer = useConfirmer()

  const valider = async () => {
    const ok = await confirmer({
      titre: `Clôturer la bande ${bande.code} ?`,
      message: 'Fin du cycle : la bande ne recevra plus de suivi ni de vente. Cette action est définitive.',
      libelleConfirmer: 'Oui, clôturer',
      ton: 'danger',
    })
    if (!ok) return
    void envoyer(async () => {
      const cloturee = await api.post<Bande>(`/Bandes/${bande.id}/cloturer`, {
        id: bande.id,
        dateCloture: date,
        observationsCloture: observations.trim() || null,
      })
      onCree(cloturee)
    })
  }

  return (
    <FormulairePanneau onSubmit={() => void valider()} erreur={erreur} envoi={envoi} libelleValider="Clôturer la bande" onAnnuler={onFermer}>
      <div className="encart-info">
        <strong>Avant de clôturer</strong>
        <p>L’application vérifie la durée du cycle, que les suivis journaliers sont complets et que la mortalité reste acceptable. Si un point bloque, il sera indiqué ici.</p>
      </div>
      <Champ libelle="Date de clôture" obligatoire erreurs={champs.dateCloture}>
        <input type="date" value={date} min={partieDate(bande.dateArrivee)} max={aujourdhui()} onChange={(e) => setDate(e.target.value)} />
      </Champ>
      <Champ libelle="Bilan de la bande" erreurs={champs.observationsCloture}>
        <textarea rows={4} maxLength={2000} value={observations} onChange={(e) => setObservations(e.target.value)} placeholder="Poids final, qualité, remarques pour la prochaine bande…" />
      </Champ>
      <p className="discret petit">
        Effectif final : <strong>{nombre(bande.nbPoussinsActuel)}</strong> sur {nombre(bande.nbPoussinsInit)} · mortalité {nombre(bande.tauxMortalite, 1)} %
      </p>
    </FormulairePanneau>
  )
}

// ===========================================================================
// Nouvelle vente
// ===========================================================================
const TYPES_VENTE = [
  { valeur: 'PouletsVivants', libelle: 'Poulets vivants', unite: 'Unite' },
  { valeur: 'PouletsNettoyes', libelle: 'Poulets nettoyés', unite: 'Unite' },
  { valeur: 'PouletsFumes', libelle: 'Poulets fumés', unite: 'Unite' },
  { valeur: 'Fientes', libelle: 'Fientes', unite: 'Sac' },
  { valeur: 'Autres', libelle: 'Autres', unite: 'Unite' },
] as const

const UNITES_VENTE = [
  { valeur: 'Unite', libelle: 'Unité' },
  { valeur: 'Kg', libelle: 'Kg' },
  { valeur: 'Sac', libelle: 'Sac' },
  { valeur: 'Camion', libelle: 'Camion' },
  { valeur: 'Rouleau', libelle: 'Rouleau' },
  { valeur: 'Paquet', libelle: 'Paquet' },
]

const MODES_PAIEMENT = [
  { valeur: 'Especes', libelle: 'Espèces' },
  { valeur: 'PaiementMobile', libelle: 'Mobile money' },
  { valeur: 'VirementBancaire', libelle: 'Virement' },
  { valeur: 'Cheque', libelle: 'Chèque' },
  { valeur: 'Credit', libelle: 'Crédit' },
] as const

/** Création (sans `existant`) ou modification d'une vente (bande et client restent fixes). */
export function FormulaireVente({
  ouvert,
  onFermer,
  onCree,
  bandeId,
  existant,
}: PropsFormulaire<Vente> & { bandeId?: number; existant?: Vente }) {
  return (
    <Panneau
      ouvert={ouvert}
      onFermer={onFermer}
      titre={existant ? 'Modifier la vente' : 'Nouvelle vente'}
      sousTitre={existant ? `${existant.clientNom ?? 'Client'} · ${existant.bandeCode ?? ''}` : 'Poulets, fientes ou autres produits vendus.'}
    >
      <CorpsVente key={existant?.id ?? 'nouvelle'} bandeId={existant?.bandeId ?? bandeId} existant={existant} onFermer={onFermer} onCree={onCree} />
    </Panneau>
  )
}

function CorpsVente({ bandeId, existant, onFermer, onCree }: Omit<PropsFormulaire<Vente>, 'ouvert'> & { bandeId?: number; existant?: Vente }) {
  const { donnees } = useDonnees(async () => {
    const [bandes, clients] = await Promise.all([api.liste<Bande>('/Bandes'), api.liste<Client>('/Clients')])
    return { bandes: bandes.filter((b) => estActive(b) || b.id === bandeId), clients: clients.filter((c) => c.estActif) }
  }, [bandeId])

  type TypeVente = (typeof TYPES_VENTE)[number]['valeur']
  type Mode = (typeof MODES_PAIEMENT)[number]['valeur']
  const [bande, setBande] = useState(bandeId ? String(bandeId) : '')
  const [client, setClient] = useState(existant ? String(existant.clientId) : '')
  const [date, setDate] = useState(existant ? partieDate(existant.dateVente) : aujourdhui())
  const [type, setType] = useState<TypeVente>((existant?.typeVente as TypeVente) ?? 'PouletsVivants')
  const [unite, setUnite] = useState(existant?.uniteMesure ?? 'Unite')
  const [quantite, setQuantite] = useState(existant ? String(existant.quantite).replace('.', ',') : '')
  const [prix, setPrix] = useState(existant ? String(existant.prixUnitaire).replace('.', ',') : '')
  const [mode, setMode] = useState<Mode>((existant?.modePaiement as Mode) ?? 'Especes')
  const [paye, setPaye] = useState(existant ? existant.estPaye : true)
  const [datePaiement, setDatePaiement] = useState(existant?.datePaiement ? partieDate(existant.datePaiement) : aujourdhui())
  const [observations, setObservations] = useState(existant?.observations ?? '')
  const { envoi, erreur, champs, envoyer, signaler } = useEnvoi()

  // L'unité suit le type choisi par l'utilisateur (une saisie existante garde la sienne)
  const changerType = (t: TypeVente) => {
    setType(t)
    setUnite(TYPES_VENTE.find((x) => x.valeur === t)!.unite)
  }

  const q = versNombre(quantite)
  const p = versNombre(prix)
  const montant = q && p ? q * p : 0
  const bandeChoisie = donnees?.bandes.find((b) => String(b.id) === bande)
  const vendPoulets = type.startsWith('Poulets')
  // En modification, les poulets de la vente d'origine sont déjà retirés de la bande : ils redeviennent disponibles
  const disponibles = (bandeChoisie?.nbPoussinsActuel ?? 0) + (existant?.typeVente.startsWith('Poulets') ? existant.quantite : 0)

  const valider = () => {
    if (!bande) return signaler('bandeId', 'Choisissez la bande.')
    if (!client) return signaler('clientId', 'Choisissez le client.')
    if (!q || q <= 0) return signaler('quantite', 'Indiquez la quantité vendue.')
    if (!p || p <= 0) return signaler('prixUnitaire', 'Indiquez le prix unitaire.')
    if (vendPoulets && bandeChoisie && q > disponibles)
      return signaler('quantite', `La bande ne compte que ${nombre(disponibles)} poulets.`)
    void envoyer(async () => {
      if (existant) {
        const modifiee = await api.put<Vente>(`/Ventes/${existant.id}`, {
          id: existant.id,
          dateVente: date,
          typeVente: type,
          quantite: q,
          uniteMesure: unite,
          prixUnitaire: p,
          modePaiement: mode,
          estPaye: paye,
          datePaiement: paye ? datePaiement : null,
          observations: observations.trim() || null,
        })
        onCree(modifiee)
        return
      }
      const vente = await api.post<Vente>('/Ventes', {
        bandeId: Number(bande),
        clientId: Number(client),
        dateVente: date,
        typeVente: type,
        quantite: q,
        uniteMesure: unite,
        prixUnitaire: p,
        modePaiement: mode,
        estPaye: paye,
        datePaiement: paye ? datePaiement : null,
        observations: observations.trim() || null,
      })
      onCree(vente)
    })
  }

  return (
    <FormulairePanneau
      onSubmit={valider}
      erreur={erreur}
      envoi={envoi}
      libelleValider={existant ? 'Enregistrer les modifications' : 'Enregistrer la vente'}
      onAnnuler={onFermer}
      resume={
        <>
          <span className="discret">Montant</span>
          <strong className="montant">{monnaie(montant)}</strong>
        </>
      }
    >
      <Ligne>
        <Champ libelle="Bande" obligatoire erreurs={champs.bandeId}>
          <select value={bande} disabled={!!existant} onChange={(e) => setBande(e.target.value)}>
            <option value="">Choisir…</option>
            {donnees?.bandes.map((b) => (
              <option key={b.id} value={b.id}>
                {b.code} · {nombre(b.nbPoussinsActuel)} sujets
              </option>
            ))}
          </select>
        </Champ>
        <Champ libelle="Client" obligatoire erreurs={champs.clientId} aide={donnees && donnees.clients.length === 0 ? 'Créez d’abord un client.' : undefined}>
          <select value={client} disabled={!!existant} onChange={(e) => setClient(e.target.value)}>
            <option value="">Choisir…</option>
            {donnees?.clients.map((c) => (
              <option key={c.id} value={c.id}>
                {c.nom}
              </option>
            ))}
          </select>
        </Champ>
      </Ligne>

      <span className="etiquette-champ">Produit vendu</span>
      <Segments libelle="Produit vendu" options={TYPES_VENTE.map((t) => ({ valeur: t.valeur, libelle: t.libelle }))} valeur={type} onChange={changerType} />

      <Ligne>
        <Champ
          libelle="Quantité"
          obligatoire
          erreurs={champs.quantite}
          aide={vendPoulets && bandeChoisie ? `${nombre(disponibles)} disponibles` : undefined}
        >
          <input type="text" inputMode="decimal" value={quantite} onChange={(e) => setQuantite(e.target.value)} placeholder="Ex. : 20" />
        </Champ>
        <Champ libelle="Unité" erreurs={champs.uniteMesure}>
          <select value={unite} onChange={(e) => setUnite(e.target.value)}>
            {UNITES_VENTE.map((u) => (
              <option key={u.valeur} value={u.valeur}>
                {u.libelle}
              </option>
            ))}
          </select>
        </Champ>
      </Ligne>
      <Ligne>
        <Champ libelle="Prix unitaire (FCFA)" obligatoire erreurs={champs.prixUnitaire}>
          <input type="text" inputMode="decimal" value={prix} onChange={(e) => setPrix(e.target.value)} placeholder="Ex. : 3500" />
        </Champ>
        <Champ libelle="Date de vente" obligatoire erreurs={champs.dateVente}>
          <input type="date" value={date} max={aujourdhui()} onChange={(e) => setDate(e.target.value)} />
        </Champ>
      </Ligne>

      <span className="etiquette-champ">Paiement</span>
      <Segments libelle="Mode de paiement" options={[...MODES_PAIEMENT]} valeur={mode} onChange={setMode} />
      <div className="ligne-interrupteur">
        <Interrupteur actif={paye} onChange={setPaye} libelle="Déjà payée" />
        {paye && (
          <Champ erreurs={champs.datePaiement} className="champ-compact">
            <input type="date" aria-label="Date de paiement" value={datePaiement} min={date} max={aujourdhui()} onChange={(e) => setDatePaiement(e.target.value)} />
          </Champ>
        )}
      </div>

      <Champ libelle="Observations" erreurs={champs.observations}>
        <textarea rows={2} maxLength={2000} value={observations} onChange={(e) => setObservations(e.target.value)} />
      </Champ>
    </FormulairePanneau>
  )
}

// ===========================================================================
// Nouvel achat
// ===========================================================================
// Unités acceptées par type (les achats qui entrent en stock sont vérifiés par la base)
const TYPES_ACHAT = [
  { valeur: 'Poussins', libelle: 'Poussins', unites: ['Unite'] },
  { valeur: 'Aliment', libelle: 'Aliment', unites: ['Kg', 'Sac'] },
  { valeur: 'Medicament', libelle: 'Médicament', unites: ['Dose', 'L', 'Unite', 'Paquet'] },
  { valeur: 'Materiel', libelle: 'Matériel', unites: ['Unite', 'Paquet'] },
  { valeur: 'Services', libelle: 'Services', unites: ['Heure', 'Unite'] },
  { valeur: 'Transport', libelle: 'Transport', unites: ['Km', 'Unite'] },
  { valeur: 'Autre', libelle: 'Autre', unites: ['Unite', 'Kg', 'L', 'Sac', 'Dose', 'Paquet'] },
] as const

const LIBELLE_UNITE: Record<string, string> = { Unite: 'Unité', Kg: 'Kg', L: 'Litre', Sac: 'Sac', Dose: 'Dose', Heure: 'Heure', Km: 'Km', Paquet: 'Paquet' }

/** Création (sans `existant`) ou modification d'un achat (fournisseur et bande restent fixes). */
export function FormulaireAchat({
  ouvert,
  onFermer,
  onCree,
  bandeId,
  existant,
}: PropsFormulaire<Achat> & { bandeId?: number; existant?: Achat }) {
  return (
    <Panneau
      ouvert={ouvert}
      onFermer={onFermer}
      titre={existant ? 'Modifier l’achat' : 'Nouvel achat'}
      sousTitre={existant ? `${existant.fournisseurNom ?? 'Fournisseur'} · ${existant.bandeCode ?? ''}` : 'L’aliment, les médicaments et le matériel achetés entrent en stock.'}
    >
      <CorpsAchat key={existant?.id ?? 'nouvel'} bandeId={existant?.bandeId ?? bandeId} existant={existant} onFermer={onFermer} onCree={onCree} />
    </Panneau>
  )
}

function CorpsAchat({ bandeId, existant, onFermer, onCree }: Omit<PropsFormulaire<Achat>, 'ouvert'> & { bandeId?: number; existant?: Achat }) {
  const { donnees } = useDonnees(async () => {
    const [bandes, fournisseurs] = await Promise.all([api.liste<Bande>('/Bandes'), api.liste<Fournisseur>('/Fournisseurs')])
    return { bandes: bandes.filter((b) => estActive(b) || b.id === bandeId), fournisseurs: fournisseurs.filter((f) => f.estActif) }
  }, [bandeId])

  type TypeAchat = (typeof TYPES_ACHAT)[number]['valeur']
  const [bande, setBande] = useState(bandeId ? String(bandeId) : '')
  const [fournisseur, setFournisseur] = useState(existant ? String(existant.fournisseurId) : '')
  const [type, setType] = useState<TypeAchat>((existant?.typeAchat as TypeAchat) ?? 'Aliment')
  const [unite, setUnite] = useState(existant?.uniteMesure ?? 'Kg')
  const [description, setDescription] = useState(existant?.description ?? '')
  const [quantite, setQuantite] = useState(existant ? String(existant.quantite).replace('.', ',') : '')
  const [prix, setPrix] = useState(existant ? String(existant.prixUnitaire).replace('.', ',') : '')
  const [date, setDate] = useState(existant ? partieDate(existant.dateAchat) : aujourdhui())
  const [facture, setFacture] = useState(existant?.factureNumero ?? '')
  const [recu, setRecu] = useState(existant ? existant.estRecu : true)
  const [dateReception, setDateReception] = useState(existant?.dateReception ? partieDate(existant.dateReception) : aujourdhui())
  const { envoi, erreur, champs, envoyer, signaler } = useEnvoi()

  const unitesDuType: readonly string[] = TYPES_ACHAT.find((t) => t.valeur === type)!.unites
  // Une saisie ancienne peut avoir une unité hors de la liste actuelle : elle reste proposée
  const unites = unitesDuType.includes(unite) ? unitesDuType : [unite, ...unitesDuType]
  const changerType = (t: TypeAchat) => {
    setType(t)
    setUnite(TYPES_ACHAT.find((x) => x.valeur === t)!.unites[0])
  }

  const q = versNombre(quantite)
  const p = versNombre(prix)
  const montant = q && p ? q * p : 0

  const valider = () => {
    if (!fournisseur) return signaler('fournisseurId', 'Choisissez le fournisseur.')
    if (!bande) return signaler('bandeId', 'Choisissez la bande concernée.')
    if (!q || q <= 0) return signaler('quantite', 'Indiquez la quantité achetée.')
    if (!p || p <= 0) return signaler('prixUnitaire', 'Indiquez le prix unitaire.')
    void envoyer(async () => {
      if (existant) {
        const modifie = await api.put<Achat>(`/Achats/${existant.id}`, {
          id: existant.id,
          dateAchat: date,
          typeAchat: type,
          description: description.trim() || null,
          quantite: q,
          uniteMesure: unite,
          prixUnitaire: p,
          factureNumero: facture.trim() || null,
          dateReception: recu ? dateReception : null,
        })
        onCree(modifie)
        return
      }
      const achat = await api.post<Achat>('/Achats', {
        fournisseurId: Number(fournisseur),
        bandeId: Number(bande),
        dateAchat: date,
        typeAchat: type,
        description: description.trim() || null,
        quantite: q,
        uniteMesure: unite,
        prixUnitaire: p,
        factureNumero: facture.trim() || null,
        dateReception: recu ? dateReception : null,
      })
      onCree(achat)
    })
  }

  return (
    <FormulairePanneau
      onSubmit={valider}
      erreur={erreur}
      envoi={envoi}
      libelleValider={existant ? 'Enregistrer les modifications' : 'Enregistrer l’achat'}
      onAnnuler={onFermer}
      resume={
        <>
          <span className="discret">Total</span>
          <strong className="montant">{monnaie(montant)}</strong>
        </>
      }
    >
      <span className="etiquette-champ">Type d’achat</span>
      <Segments libelle="Type d’achat" options={TYPES_ACHAT.map((t) => ({ valeur: t.valeur, libelle: t.libelle }))} valeur={type} onChange={changerType} />

      <Ligne>
        <Champ libelle="Fournisseur" obligatoire erreurs={champs.fournisseurId} aide={donnees && donnees.fournisseurs.length === 0 ? 'Créez d’abord un fournisseur.' : undefined}>
          <select value={fournisseur} disabled={!!existant} onChange={(e) => setFournisseur(e.target.value)}>
            <option value="">Choisir…</option>
            {donnees?.fournisseurs.map((f) => (
              <option key={f.id} value={f.id}>
                {f.nom}
              </option>
            ))}
          </select>
        </Champ>
        <Champ libelle="Pour la bande" obligatoire erreurs={champs.bandeId}>
          <select value={bande} disabled={!!existant} onChange={(e) => setBande(e.target.value)}>
            <option value="">Choisir…</option>
            {donnees?.bandes.map((b) => (
              <option key={b.id} value={b.id}>
                {b.code}
              </option>
            ))}
          </select>
        </Champ>
      </Ligne>

      <Champ libelle="Description" erreurs={champs.description} aide={type === 'Poussins' ? 'Les poussins achetés s’ajoutent à l’effectif de la bande.' : undefined}>
        <input type="text" maxLength={500} value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Ex. : Aliment démarrage 1er âge" />
      </Champ>

      <Ligne>
        <Champ libelle="Quantité" obligatoire erreurs={champs.quantite}>
          <input type="text" inputMode="decimal" value={quantite} onChange={(e) => setQuantite(e.target.value)} placeholder="Ex. : 50" />
        </Champ>
        <Champ libelle="Unité" erreurs={champs.uniteMesure}>
          <select value={unite} onChange={(e) => setUnite(e.target.value)}>
            {unites.map((u) => (
              <option key={u} value={u}>
                {LIBELLE_UNITE[u]}
              </option>
            ))}
          </select>
        </Champ>
      </Ligne>
      <Ligne>
        <Champ libelle="Prix unitaire (FCFA)" obligatoire erreurs={champs.prixUnitaire}>
          <input type="text" inputMode="decimal" value={prix} onChange={(e) => setPrix(e.target.value)} placeholder="Ex. : 350" />
        </Champ>
        <Champ libelle="Date d’achat" obligatoire erreurs={champs.dateAchat}>
          <input type="date" value={date} max={aujourdhui()} onChange={(e) => setDate(e.target.value)} />
        </Champ>
      </Ligne>

      <Champ libelle="N° de facture" erreurs={champs.factureNumero}>
        <input type="text" maxLength={50} value={facture} onChange={(e) => setFacture(e.target.value)} placeholder="Facultatif" />
      </Champ>
      <div className="ligne-interrupteur">
        <Interrupteur actif={recu} onChange={setRecu} libelle="Déjà reçu" />
        {recu && (
          <Champ erreurs={champs.dateReception} className="champ-compact">
            <input type="date" aria-label="Date de réception" value={dateReception} min={date} max={aujourdhui()} onChange={(e) => setDateReception(e.target.value)} />
          </Champ>
        )}
      </div>
    </FormulairePanneau>
  )
}

// ===========================================================================
// Nouveau client / fournisseur
// ===========================================================================
const TYPES_CLIENT = [
  { valeur: 'Particulier', libelle: 'Particulier' },
  { valeur: 'Restaurant', libelle: 'Restaurant' },
  { valeur: 'Marche', libelle: 'Marché' },
  { valeur: 'Grossiste', libelle: 'Grossiste' },
  { valeur: 'Transformateur', libelle: 'Transformateur' },
  { valeur: 'Autre', libelle: 'Autre' },
]

const TYPES_FOURNISSEUR = [
  { valeur: 'Poussiniere', libelle: 'Poussinière' },
  { valeur: 'Aliment', libelle: 'Aliment' },
  { valeur: 'Medicament', libelle: 'Médicaments' },
  { valeur: 'Materiel', libelle: 'Matériel' },
  { valeur: 'Services', libelle: 'Services' },
  { valeur: 'Transport', libelle: 'Transport' },
  { valeur: 'Autre', libelle: 'Autre' },
]

export function FormulaireClient({ ouvert, onFermer, onCree }: PropsFormulaire<Client>) {
  return (
    <Panneau ouvert={ouvert} onFermer={onFermer} titre="Nouveau client">
      <CorpsContact<Client>
        types={TYPES_CLIENT}
        route="/Clients"
        champType="typeClient"
        avecInterlocuteur={false}
        aideNom="Lettres, espaces, tirets et apostrophes uniquement."
        libelleValider="Créer le client"
        onFermer={onFermer}
        onCree={onCree}
      />
    </Panneau>
  )
}

export function FormulaireFournisseur({ ouvert, onFermer, onCree }: PropsFormulaire<Fournisseur>) {
  return (
    <Panneau ouvert={ouvert} onFermer={onFermer} titre="Nouveau fournisseur">
      <CorpsContact<Fournisseur>
        types={TYPES_FOURNISSEUR}
        route="/Fournisseurs"
        champType="typeFournisseur"
        avecInterlocuteur
        libelleValider="Créer le fournisseur"
        onFermer={onFermer}
        onCree={onCree}
      />
    </Panneau>
  )
}

function CorpsContact<T>({
  types,
  route,
  champType,
  avecInterlocuteur,
  libelleValider,
  aideNom,
  onFermer,
  onCree,
}: Omit<PropsFormulaire<T>, 'ouvert'> & {
  types: { valeur: string; libelle: string }[]
  route: string
  champType: string
  avecInterlocuteur: boolean
  libelleValider: string
  aideNom?: string
}) {
  const [nom, setNom] = useState('')
  const [type, setType] = useState(types[0].valeur)
  const [telephone, setTelephone] = useState('')
  const [email, setEmail] = useState('')
  const [adresse, setAdresse] = useState('')
  const [interlocuteur, setInterlocuteur] = useState('')
  const [notes, setNotes] = useState('')
  const { envoi, erreur, champs, envoyer, signaler } = useEnvoi()

  const valider = () => {
    if (!nom.trim()) return signaler('nom', 'Indiquez le nom.')
    void envoyer(async () => {
      const cree = await api.post<T>(route, {
        nom: nom.trim(),
        [champType]: type,
        telephone: telephone.trim() || null,
        email: email.trim() || null,
        adresse: adresse.trim() || null,
        ...(avecInterlocuteur ? { contactPersonne: interlocuteur.trim() || null } : {}),
        notes: notes.trim() || null,
      })
      oublierNoms()
      onCree(cree)
    })
  }

  return (
    <FormulairePanneau onSubmit={valider} erreur={erreur} envoi={envoi} libelleValider={libelleValider} onAnnuler={onFermer}>
      <Champ libelle="Nom" obligatoire erreurs={champs.nom} aide={aideNom}>
        <input type="text" maxLength={100} value={nom} onChange={(e) => setNom(e.target.value)} autoComplete="organization" />
      </Champ>
      <span className="etiquette-champ">Type</span>
      <Segments libelle="Type" options={types} valeur={type} onChange={setType} />
      <Ligne>
        <Champ libelle="Téléphone" erreurs={champs.telephone}>
          <input type="tel" inputMode="tel" maxLength={20} value={telephone} onChange={(e) => setTelephone(e.target.value)} placeholder="6 90 00 00 00" />
        </Champ>
        <Champ libelle="E-mail" erreurs={champs.email}>
          <input type="email" inputMode="email" maxLength={100} value={email} onChange={(e) => setEmail(e.target.value)} />
        </Champ>
      </Ligne>
      {avecInterlocuteur && (
        <Champ libelle="Interlocuteur" erreurs={champs.contactPersonne}>
          <input type="text" maxLength={100} value={interlocuteur} onChange={(e) => setInterlocuteur(e.target.value)} placeholder="Nom de la personne à contacter" />
        </Champ>
      )}
      <Champ libelle="Adresse" erreurs={champs.adresse}>
        <input type="text" maxLength={500} value={adresse} onChange={(e) => setAdresse(e.target.value)} />
      </Champ>
      <Champ libelle="Notes" erreurs={champs.notes}>
        <textarea rows={2} maxLength={1000} value={notes} onChange={(e) => setNotes(e.target.value)} />
      </Champ>
    </FormulairePanneau>
  )
}
