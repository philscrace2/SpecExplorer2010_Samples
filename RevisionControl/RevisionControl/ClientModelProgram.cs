using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Modeling;

namespace RevisionControl
{    
    [Feature("Scenario1a")]
    public static class User
    {
        /// <summary>
        /// The last version checked out by each user
        /// </summary>
        static Map<string, int> version = new Map<string, int>();

        /// <summary>
        /// The pending changes made by each user that have not yet been committed to the repository
        /// </summary>
        static Map<string, Map<string, Op>> revisions = new Map<string, Map<string, Op>>();

        /// <summary>
        /// The files that must be resolved before check in may complete
        /// </summary>
        static Map<string, Set<string>> conflicts = new Map<string, Set<string>>();

        /// <summary>
        /// The set of users with pending commit operations. Simultaneous commits result in a race condition. 
        /// </summary>
        static Set<string> commitPending = new Set<string>();

        // State queries

        /// <summary>
        /// Is this user waiting for a Commit action to respond?
        /// </summary>
        static bool CommitPending(string user) { return commitPending.Contains(user); }

        /// <summary>
        /// Has this user performed at least one Checkout action?
        /// </summary>
        static bool IsUser(string user) { return version.ContainsKey(user); }

        /// <summary>
        /// Are the actions Checkout, Commit, Resolve, Edit and Revert
        /// </summary>
        static bool CanStep(string user) { return IsUser(user) && !CommitPending(user); }

        /// <summary>
        /// Which users have performed at least one Checkout action?
        /// </summary>
        public static Set<string> Users()
        {
            return new Set<string>(version.Keys);
        }

        //public static Set<string> Users() { return users; }
        //public static Set<string> Files() { return files; }
        //public static Set<int> Revisions() { return revisions; }

        static int new_version;

        [Rule]
        static void Checkout(string user)
        {
            Condition.IsTrue(!IsUser(user));

            int newVersion = Repository.currentRevision;

            version = version.Add(user, newVersion);
            revisions = revisions.Add(user, new Map<string, Op>());
            conflicts = conflicts.Add(user, new Set<string>());

            if (newVersion > version[user])
            {
                IdentifyConflicts(user, version[user]);
                version = version.Override(user, newVersion);
            }
            //else
            //{
            //    version = version.Add(user, newVersion);
            //    revisions = revisions.Add(user, new Map<string, Op>());
            //    conflicts = conflicts.Add(user, new Set<string>());
            //}
        }

        static void IdentifyConflicts(string user, int currentVersion)
        {
            Set<string> userConflicts = conflicts[user];
            foreach (KeyValuePair<string, Op> revision in revisions[user])
            {
                string file = revision.Key;
                Op op = revision.Value;

                if (!userConflicts.Contains(file) &&
                    currentVersion < Repository.FileVersion(file) &&
                    Repository.FileExists(file, currentVersion) &&
                    op != Op.Delete)
                    userConflicts = userConflicts.Add(file);
            }
            conflicts = conflicts.Override(user, userConflicts);
        }

        [Rule]
        static void Resolve([Domain("Users")] string user, string file)
        {
            Condition.IsTrue(CanStep(user) && conflicts[user].Contains(file));

            Set<string> remainingFiles = conflicts[user].Remove(file);
            conflicts = conflicts.Override(user, remainingFiles);
        }

        [Rule]
        static void Edit([Domain("Users")] string user, string file, Op op)
        {
            Condition.IsTrue(Repository.currentRevision < 2);
            Condition.IsTrue((CanStep(user) &&
                    !revisions[user].ContainsKey(file) &&
                    (Repository.FileExists(file, version[user]) ? op != Op.Add : op == Op.Add)));

            Map<string, Op> userRevisions = revisions[user];
            revisions = revisions.Override(user, userRevisions.Add(file, (op == Op.Delete ? Op.Delete : Op.Add)));
        }

        [Rule]
        static void Revert([Domain("Users")] string user, string file)
        {
            Condition.IsTrue(CanStep(user) && revisions[user].ContainsKey(file));

            revisions = revisions.Override(user, revisions[user].Remove(file));
            conflicts = conflicts.Override(user, conflicts[user].Remove(file));
        }

        [Rule]
        static void Commit([Domain("Users")] string user)
        {
            Condition.IsTrue(CanStep(user));

            commitPending = commitPending.Add(user);
        }

        [Rule]
        static void CommitComplete(string user, [Domain("NextVersion")] int newVersion)
        {
            Condition.IsTrue(IsUser(user) && CommitPending(user) && FileConflicts(user).Count <= 0
                && newVersion == (revisions[user].Count <= 0 ?
                       Repository.currentRevision : (Repository.currentRevision + 1)));

            Map<string, Op> userRevisions = revisions[user];

            new_version = newVersion;

            version = version.Override(user, newVersion);
            revisions = revisions.Override(user, new Map<string, Op>());
            commitPending = commitPending.Remove(user);
            Repository.Commit(user, "Check in", userRevisions);
        }

        public static IEnumerable<Set<string>> ResolveSets()
        {
            Set<Set<string>> result = new Set<Set<string>>();
            foreach (string user in Users())
                if (commitPending.Contains(user))
                {
                    Set<string> fileConflicts = FileConflicts(user);
                    if (fileConflicts.Count > 0)
                        result = result.Add(fileConflicts);
                }
            return result;
        }

        static Set<string> FileConflicts(string user)
        {
            Set<string> result = conflicts[user];

            foreach (KeyValuePair<string, Op> revision in revisions[user])
            {
                string file = revision.Key;
                Op op = revision.Value;

                if (version[user] < Repository.FileVersion(file) &&
                    Repository.FileExists(file) &&
                    op != Op.Delete)
                    result = result.Add(file);
            }
            return result;
        }

        [Rule]
        static void MustResolve([Domain("Users")] string user,
                                [Domain("ResolveSets")] Set<string> files)
        {
            Condition.IsTrue(IsUser(user) &&
                   CommitPending(user) &&
                   files.Count > 0 && Object.Equals(files, FileConflicts(user)));

            commitPending = commitPending.Remove(user);
            IdentifyConflicts(user, version[user]);
            version = version.Override(user, Repository.currentRevision);
        }


        public static IEnumerable<int> NextVersion()
        {
            return new SetContainer<int>(Repository.currentRevision + 1);
        }
        

        [Probe]
        static int Probe()
        {
            return new_version;
        }

        //add
        //blame (praise, annotate, ann)
        //cat
        //checkout (co)
        //cleanup
        //commit (ci)
        //copy (cp)
        //delete (del, remove, rm)
        //diff (di)
        //export
        //help (?, h)
        //import
        //info
        //list (ls)
        //lock
        //log
        //merge
        //mkdir
        //move (mv, rename, ren)
        //propdel (pdel, pd)
        //propedit (pedit, pe)
        //propget (pget, pg)
        //proplist (plist, pl)
        //propset (pset, ps)
        //resolved
        //revert
        //status (stat, st)
        //switch (sw)
        //unlock
        //update (up)

    }

    /// <summary>
    /// Revision control operations
    /// </summary>


    /// <summary>
    /// A data record representing a (operation, revisionNumber) pair.
    /// </summary>
    public class Revision : CompoundValue
    {
        public readonly Op op;                     // the operation performed
        public readonly int revisionNumber;        // the revision number of the database for this change

        public Revision(Op op, int revisionNumber)
        {
            this.op = op;
            this.revisionNumber = revisionNumber;
        }
    }

    public enum Op { Add, Delete, Change }

    public static class Repository
    {
        /// <summary>
        /// The number of times a change has been committed to the repository
        /// </summary>
        public static int currentRevision = 0;

        /// <summary>
        /// The database of revisions. Each entry in the map associates a file name with
        /// the change log for that file. A change log is a sequence of revisions. The
        /// first element of a change log is the most recent revision.
        /// </summary>
        static Map<string, Sequence<Revision>> db = new Map<string, Sequence<Revision>>();

        /// <summary>
        /// For each revision, the check-in message supplied by the user for that revision.
        /// </summary>
        static Sequence<string> revisionMessages = new Sequence<string>();

        /// <summary>
        /// For each revision, the client who requested that revision.
        /// </summary>
        static Sequence<string> revisionClients = new Sequence<string>();

        #region Invariants

        [AcceptingStateCondition]
        static bool DbEntriesEqual()
        {
            return Repository.currentRevision == 5;
        }

        [StateInvariant]
        //[Requirement("Revision number of the repository must be nonnegative.")]
        static bool ValidRevision() { return currentRevision >= 0; }

        [StateInvariant]
        //[Requirement("The number of check-in messages must match the currrent revision.")]
        static bool ValidMessageCount() { return revisionMessages.Count == currentRevision; }

        [StateInvariant]
        //[Requirement("The number of check-in clients must match the currrent revision.")]
        static bool ValidClientCount() { return revisionClients.Count == currentRevision; }

        [StateInvariant]
        //[Requirement("The revision numbers of every change log in the repository must be a valid revision number.")]
        static bool ValidDatabaseRevisions()
        {
            foreach (Sequence<Revision> changeLog in db.Values)
            {
                foreach (Revision revision in changeLog)
                {
                    int rev = revision.revisionNumber;
                    if (rev < 0 || rev > currentRevision)
                        return false;
                }
            }
            return true;
        }

        //[StateInvariant]
        ////[Requirement("The revision numbers of every change log in the repository must decreasing.")]
        //static bool ValidDatabaseRevisionOrder()
        //{
        //    foreach (Sequence<Revision> changeLog in db.Values)
        //    {
        //        int prevRev = -1;
        //        foreach (Revision revision in changeLog)
        //        {
        //            int rev = revision.revisionNumber;
        //            if (prevRev > -1 && rev <= prevRev)
        //                return false;
        //            prevRev = rev;
        //        }
        //    }
        //    return true;
        //}
        

        [StateInvariant]
        //[Requirement("Every change list in the repository must contain at least one change.")]
        static bool ValidDatabaseRevisionLength()
        {
            foreach (Sequence<Revision> changeLog in db.Values)
            {
                if (changeLog.Count < 1)
                    return false;
            }
            return true;
        }

        [StateInvariant]
        //[Requirement("The last element in every change list in the repository must be an Add operation.")]
        static bool ValidDatabaseRevisionTail()
        {
            foreach (Sequence<Revision> changeLog in db.Values)
                if (changeLog.Count > 0 && Last(changeLog) != Op.Add)
                    return false;

            return true;
        }

        static Op Last(Sequence<Revision> seq)
        {
            return seq[seq.Count -1].op;
        }

        #endregion

        [Probe]
        public static int Revision()
        {
            return currentRevision;
        }


        static Revision Head(Sequence<Revision> seq)
        {
            return seq[0];
        }

        /// <summary>
        /// Returns the version number of the most recent checkin of this file
        /// </summary>
        public static int FileVersion(string file)
        {
            Sequence<Revision> revisions;
            return (db.TryGetValue(file, out revisions) ?
                        Head(revisions).revisionNumber : -1);

        }

        public static bool FileExists(string file)
        {
            Sequence<Revision> revisions;
            if (db.TryGetValue(file, out revisions))
                return (Head(revisions).op != Op.Delete);
            else
                return false;
        }

        /// <summary>
        /// Does the file exist in the given version? 
        /// </summary>
        public static bool FileExists(string file, int version)
        {
            Sequence<Revision> revisions;
            if (db.TryGetValue(file, out revisions))
                foreach (Revision r in revisions)
                    if (r.revisionNumber <= version)
                        return (r.op != Op.Delete);
            return false;
        }

        public static Set<string> CheckForConflicts(int clientRevisionNumber, Map<string, Op> changes)
        {
            Set<string> result = new Set<string>();
            foreach (KeyValuePair<string, Op> change in changes)
            {
                string file = change.Key;
                Op op = change.Value;

                if (FileExists(file) && op == Op.Change && FileVersion(file) > clientRevisionNumber)
                    result = result.Add(file);
            }
            return result;
        }

        public static int Commit(string client, string message, Map<string, Op> changes)
        {
            Condition.IfThen(string.IsNullOrEmpty(client), false);
            Condition.IfThen(string.IsNullOrEmpty(message), false);

            foreach (KeyValuePair<string, Op> change in changes)
            {
                string file = change.Key;
                Op op = change.Value;

                Condition.IfThen(!db.ContainsKey(file) && op != Op.Add, false);
            }

            foreach (KeyValuePair<string, Op> change in changes)
            {
                string file = change.Key;
                Op op = change.Value;
                Revision revision = new Revision(op, currentRevision + 1);
                Sequence<Revision> revisions;

                if (db.TryGetValue(file, out revisions))
                    db = db.Override(file, revisions.Insert(0, revision));
                else
                    db = db.Add(file, new Sequence<Revision>(revision));
            }

            currentRevision = currentRevision + 1;
            revisionMessages = revisionMessages.Add(message);
            revisionClients = revisionClients.Add(client);

            return currentRevision;
        }

    }

    [Feature("Scenario1")]
    public static class Scenario1
    {
        public static readonly Set<string> users = new Set<string>("alice");
        public static readonly Set<string> files = new Set<string>("file1");
        public static readonly Set<int> revisions = new Set<int>(0, 1, 2, 3, 4);

        public static Set<string> Users() { return users; }
        public static Set<string> Files() { return files; }
        public static Set<int> Revisions() { return revisions; }

        [Rule]
        static void Checkout([Domain("Users")] string users) { }

        [Rule]
        public static void Revert([Domain("Users")] string users, [Domain("Files")] string files) { }

        [Rule]
        static void Edit([Domain("Users")] string user, [Domain("Files")] string file, Op op) { }

        static bool EditEnabled() { return Repository.currentRevision < 5; }

        [Rule]
        static void CommitComplete([Domain("Users")] string users, [Domain("Revisions")] int newRevision) { }

        [Rule]
        static void Commit([Domain("Users")] string user) { }
    }


}



