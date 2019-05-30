using System;
using System.Collections.Generic;
using System.Text;

namespace PullRequestLibrary.Generated.PRThread
{

    public class Rootobject
    {
        public Value[] value { get; set; }
        public int count { get; set; }
    }

    public class Value
    {
        public Pullrequestthreadcontext pullRequestThreadContext { get; set; }
        public int id { get; set; }
        public DateTime publishedDate { get; set; }
        public DateTime lastUpdatedDate { get; set; }
        public Comment[] comments { get; set; }
        public string status { get; set; }
        public Threadcontext threadContext { get; set; }
        public Properties properties { get; set; }
        public Identities identities { get; set; }
        public bool isDeleted { get; set; }
        public _Links1 _links { get; set; }
    }

    public class Pullrequestthreadcontext
    {
        public Iterationcontext iterationContext { get; set; }
    }

    public class Iterationcontext
    {
        public int firstComparingIteration { get; set; }
        public int secondComparingIteration { get; set; }
    }

    public class Threadcontext
    {
        public string filePath { get; set; }
        public Rightfilestart rightFileStart { get; set; }
        public Rightfileend rightFileEnd { get; set; }
    }

    public class Rightfilestart
    {
        public int line { get; set; }
        public int offset { get; set; }
    }

    public class Rightfileend
    {
        public int line { get; set; }
        public int offset { get; set; }
    }

    public class Properties
    {
        public Codereviewthreadtype CodeReviewThreadType { get; set; }
        public Codereviewrefname CodeReviewRefName { get; set; }
        public Codereviewrefnewcommits CodeReviewRefNewCommits { get; set; }
        public Codereviewrefnewcommitscount CodeReviewRefNewCommitsCount { get; set; }
        public Codereviewrefnewheadcommit CodeReviewRefNewHeadCommit { get; set; }
        public Codereviewrefupdatedbyidentity CodeReviewRefUpdatedByIdentity { get; set; }
        public MicrosoftTeamfoundationDiscussionUniqueid MicrosoftTeamFoundationDiscussionUniqueID { get; set; }
    }

    public class Codereviewthreadtype
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class Codereviewrefname
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class Codereviewrefnewcommits
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class Codereviewrefnewcommitscount
    {
        public string type { get; set; }
        public int value { get; set; }
    }

    public class Codereviewrefnewheadcommit
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class Codereviewrefupdatedbyidentity
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class MicrosoftTeamfoundationDiscussionUniqueid
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class Identities
    {
        public _1 _1 { get; set; }
    }

    public class _1
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public _Links _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class _Links
    {
        public Avatar avatar { get; set; }
    }

    public class Avatar
    {
        public string href { get; set; }
    }

    public class _Links1
    {
        public Self self { get; set; }
        public Repository repository { get; set; }
    }

    public class Self
    {
        public string href { get; set; }
    }

    public class Repository
    {
        public string href { get; set; }
    }

    public class Comment
    {
        public int id { get; set; }
        public int parentCommentId { get; set; }
        public Author author { get; set; }
        public DateTime publishedDate { get; set; }
        public DateTime lastUpdatedDate { get; set; }
        public DateTime lastContentUpdatedDate { get; set; }
        public bool isDeleted { get; set; }
        public string commentType { get; set; }
        public object[] usersLiked { get; set; }
        public _Links3 _links { get; set; }
        public string content { get; set; }
    }

    public class Author
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public _Links2 _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class _Links2
    {
        public Avatar1 avatar { get; set; }
    }

    public class Avatar1
    {
        public string href { get; set; }
    }

    public class _Links3
    {
        public Self1 self { get; set; }
        public Repository1 repository { get; set; }
        public Threads threads { get; set; }
    }

    public class Self1
    {
        public string href { get; set; }
    }

    public class Repository1
    {
        public string href { get; set; }
    }

    public class Threads
    {
        public string href { get; set; }
    }

}
